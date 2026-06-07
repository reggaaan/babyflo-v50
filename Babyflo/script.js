// --- DOM ELEMENTS (initialized after DOM is ready) ---
let openCartBtn;
let closeCartBtn;
let cartSidebar;
let productGrid;
let cartItemsContainer;
let cartCountSpan;
let cartTotalSpan;
let voucherInput;
let applyVoucherBtn;
let dbContactForm;

// --- APP STATE ---
let cart = [];
let discount = 0; 

// Simple escaper to avoid injecting raw HTML (basic)
function escapeHtml(str) {
    if (str === null || str === undefined) return '';
    return String(str).replace(/[&<>"']/g, s => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[s]));
}

// --- INITIALIZATION ---
document.addEventListener('DOMContentLoaded', () => {
    // initialize DOM refs
    openCartBtn = document.getElementById('openCart');
    closeCartBtn = document.getElementById('closeCart');
    cartSidebar = document.getElementById('cartSidebar');
    productGrid = document.getElementById('productGrid');
    cartItemsContainer = document.getElementById('cartItems');
    cartCountSpan = document.getElementById('cart-count');
    cartTotalSpan = document.getElementById('cart-total');
    voucherInput = document.getElementById('voucherInput');
    applyVoucherBtn = document.getElementById('applyVoucher');
    dbContactForm = document.getElementById('dbContactForm');

    // 1. Session Auth Logic
    const userName = localStorage.getItem('customerName');
    const userEmail = localStorage.getItem('customerEmail');
    const signInBtn = document.getElementById('signInBtn');
    const userProfileActive = document.getElementById('userProfileActive');
    const welcomeUserName = document.getElementById('welcomeUserName');
    const logoutBtn = document.getElementById('logoutBtn');

    if (userName) {
        if (signInBtn) signInBtn.style.display = 'none';
        if (userProfileActive) userProfileActive.style.display = 'flex';
        if (welcomeUserName) welcomeUserName.innerText = `Hello, ${userName.split(' ')[0]}!`;

        if (userEmail === 'admin@babyflo.com' && !document.getElementById('returnAdminBtn') && userProfileActive && logoutBtn) {
            const adminBtn = document.createElement('a');
            adminBtn.id = 'returnAdminBtn';
            adminBtn.href = 'admin.html';
            adminBtn.innerHTML = '<i class="fa-solid fa-shield-halved"></i> Dashboard';
            adminBtn.style.cssText = 'background: linear-gradient(to right, #ff8ab0, #ffc2d4); color: white; padding: 5px 12px; border-radius: 6px; font-weight: 600; text-decoration: none; font-size: 0.9rem; margin-right: 5px;';
            userProfileActive.insertBefore(adminBtn, logoutBtn);
        }
    }

    if (logoutBtn) {
        logoutBtn.addEventListener('click', () => {
            localStorage.clear();
            window.location.href = 'index.html';
        });
    }

    // 2. Load Core Functions
    loadDatabaseProducts();
    setupGridListener();
    setupVoucherLogic();
    setupHeroScrolls();
    setupContactForm();
});

// --- PRODUCT LOADING & RENDERING ---
async function loadDatabaseProducts() {
    const gridContainer = productGrid || document.getElementById('productGrid');
    if (!gridContainer) return;

    try {
        const response = await fetch('http://localhost:5190/api/products');
        if (!response.ok) throw new Error(`Status ${response.status}`);
        const products = await response.json();

        gridContainer.innerHTML = products.map(p => {
            const name = escapeHtml(p.name);
            const desc = escapeHtml(p.description || "Gentle and safe fragrance.");
            const img = escapeHtml(p.imageUrl || '');
            const price = (typeof p.price === 'number') ? p.price.toFixed(2) : '0.00';
            const disabled = !p.inStock ? 'disabled' : '';
            const soldBadge = !p.inStock ? '<span class="badge-sold">Sold Out</span>' : '';
            const bestBadge = p.isBestSeller ? '<span class="badge-best">Best Seller</span>' : '';
            return `
            <div class="product-card ${!p.inStock ? 'disabled' : ''}">
                ${bestBadge}
                ${soldBadge}
                <img src="${img}" alt="${name}">
                <div class="product-info">
                    <h3>${name}</h3>
                    <p>${desc}</p>
                    <div class="price-row">
                        <h4>₱${price}</h4>
                    </div>
                    <button type="button" class="add-cart-btn" 
                            data-name="${escapeHtml(p.name)}" 
                            data-price="${Number(p.price) || 0}"
                            ${disabled}>
                        ${p.inStock ? 'Add to Cart' : 'Unavailable'}
                    </button>
                </div>
            </div>
        `; 
        }).join('');
    } catch (error) {
        console.error("Database connection failed:", error);
        gridContainer.innerHTML = `<p style="grid-column: 1 / -1; text-align: center; color: #ff7fa2;">Server Offline.</p>`;
    }
}

// --- CART & INTERACTION LOGIC ---
function setupGridListener() {
    if (!productGrid) productGrid = document.getElementById('productGrid');
    if (!productGrid) return;
    productGrid.addEventListener('click', (e) => {
        const target = e.target.closest('.add-cart-btn');
        if (!target) return;
        const isUserLoggedIn = localStorage.getItem('customerName') !== null;
        if (!isUserLoggedIn) {
            alert("Please sign in to start shopping!");
            window.location.href = 'login.html';
            return;
        }
        const name = target.getAttribute('data-name');
        const price = parseFloat(target.getAttribute('data-price')) || 0;
        addToCart(name, price);
    });
}

function addToCart(name, price) {
    const existingItem = cart.find(item => item.name === name);
    if (existingItem) existingItem.quantity += 1;
    else cart.push({ name, price, quantity: 1 });
    updateCartUI();
    if (cartSidebar) cartSidebar.classList.add('active');
}

function updateCartUI() {
    if (!cartItemsContainer) cartItemsContainer = document.getElementById('cartItems');
    if (!cartItemsContainer) return;
    cartItemsContainer.innerHTML = '';
    let totalItems = 0;
    let subtotal = 0;

    cart.forEach((item, index) => {
        totalItems += item.quantity;
        subtotal += item.price * item.quantity;
        const cartItem = document.createElement('div');
        cartItem.className = 'cart-item';
        cartItem.innerHTML = `
            <div><h4>${escapeHtml(item.name)}</h4><small>₱${Number(item.price).toFixed(2)} x ${item.quantity}</small></div>
            <div style="display: flex; gap: 5px;">
                <button type="button" onclick="changeQuantity(${index}, 1)">+</button>
                <button type="button" onclick="changeQuantity(${index}, -1)">-</button>
                <button type="button" onclick="removeItem(${index})"><i class="fa-solid fa-trash"></i></button>
            </div>
        `;
        cartItemsContainer.appendChild(cartItem);
    });

    if (cartCountSpan) cartCountSpan.textContent = totalItems;
    if (cartTotalSpan) cartTotalSpan.textContent = (subtotal * (1 - discount / 100)).toFixed(2);
}

// --- GLOBALS FOR HTML ONCLICK ---
window.changeQuantity = (index, amount) => {
    if (!cart[index]) return;
    cart[index].quantity += amount;
    if (cart[index].quantity <= 0) cart.splice(index, 1);
    updateCartUI();
};

window.removeItem = (index) => {
    if (index < 0 || index >= cart.length) return;
    cart.splice(index, 1);
    updateCartUI();
};

// --- UTILITIES ---
function setupHeroScrolls() {
    document.getElementById('shopNowBtn')?.addEventListener('click', () => document.getElementById('products')?.scrollIntoView({ behavior: 'smooth' }));
    document.getElementById('learnMoreBtn')?.addEventListener('click', () => document.getElementById('about')?.scrollIntoView({ behavior: 'smooth' }));
}

function setupVoucherLogic() {
    if (!applyVoucherBtn) applyVoucherBtn = document.getElementById('applyVoucher');
    if (!voucherInput) voucherInput = document.getElementById('voucherInput');
    applyVoucherBtn?.addEventListener('click', () => {
        if (!voucherInput) return;
        if (voucherInput.value.trim().toUpperCase() === 'BABY15') {
            discount = 15;
            updateCartUI();
        } else alert('Invalid code!');
    });
}

function setupContactForm() {
    if (!dbContactForm) dbContactForm = document.getElementById('dbContactForm');
    if (!dbContactForm) return;
    dbContactForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        try {
            const nameEl = document.getElementById('contactName');
            const emailEl = document.getElementById('contactEmail');
            const msgEl = document.getElementById('contactMessage');
            if (!nameEl || !emailEl || !msgEl) { alert('Form fields missing'); return; }

            const body = {
                name: nameEl.value.trim(),
                email: emailEl.value.trim(),
                message: msgEl.value.trim()
            };

            const res = await fetch('http://localhost:5190/api/contact/submit', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(body)
            });
            if (res.ok) { alert("Message Sent!"); dbContactForm.reset(); }
            else {
                const txt = await res.text();
                console.error('Contact submit failed:', res.status, txt);
                alert('Failed to send message.');
            }
        } catch (err) {
            console.error('Contact submit error:', err);
            alert('Unable to send message right now.');
        }
    });
}