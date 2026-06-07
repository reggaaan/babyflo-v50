const API_URL = "http://localhost:5190/api";

document.addEventListener('DOMContentLoaded', () => {
    loadProducts();

    // Logout logic (kept for compatibility with admin.html)
    const logoutBtn = document.getElementById('adminLogout');
    if (logoutBtn) {
        logoutBtn.addEventListener('click', () => {
            localStorage.removeItem('customerName');
            localStorage.removeItem('customerEmail');
            window.location.href = 'login.html';
        });
    }
});

function escapeHtml(str) {
    if (str === null || str === undefined) return '';
    return String(str).replace(/[&<>"']/g, s => {
        return ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[s]);
    });
}

async function loadProducts() {
    try {
        const res = await fetch(`${API_URL}/products`);
        if (!res.ok) throw new Error(`Failed to load products (${res.status})`);
        const products = await res.json();
        const tbody = document.getElementById('adminProductTable');
        if (!tbody) return;

        tbody.innerHTML = products.map(p => {
            const id = Number(p.id) || 0;
            const name = escapeHtml(p.name);
            const price = Number(p.price) ? Number(p.price).toFixed(2) : '0.00';
            const discount = escapeHtml(p.discount || 0);
            const inStock = !!p.inStock;
            return `
                <tr>
                    <td><strong>${name}</strong></td>
                    <td>₱${price}</td>
                    <td><input type="number" class="discount-input" value="${discount}" data-id="${id}">%</td>
                    <td>
                        <button type="button" class="badge badge-stock ${inStock ? 'active' : ''}" 
                                onclick="toggleStock(${id}, ${inStock ? 'false' : 'true'})">
                            ${inStock ? 'In Stock' : 'Out of Stock'}
                        </button>
                    </td>
                    <td>
                        <button type="button" class="action-btn btn-delete" onclick="deleteProduct(${id})">
                            <i class="fa-solid fa-trash"></i>
                        </button>
                    </td>
                </tr>
            `;
        }).join('');
    } catch (err) {
        console.error("Error loading products:", err);
        alert("Unable to load products. Check server/API.");
    }
}

async function addNewProduct() {
    const nameEl = document.getElementById('newProdName');
    const descEl = document.getElementById('newProdDesc');
    const priceEl = document.getElementById('newProdPrice');
    const imgEl = document.getElementById('newProdImg');

    if (!nameEl || !priceEl || !descEl || !imgEl) {
        alert("Form elements missing.");
        return;
    }

    const name = nameEl.value.trim();
    const description = descEl.value.trim();
    const price = parseFloat(priceEl.value);
    const imageUrl = imgEl.value.trim();

    if (!name) { alert("Product name is required."); return; }
    if (!isFinite(price) || price <= 0) { alert("Enter a valid price."); return; }

    const product = { name, description, price, imageUrl, inStock: true };

    try {
        const res = await fetch(`${API_URL}/products`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(product)
        });
        if (!res.ok) throw new Error(`Publish failed (${res.status})`);

        // Clear form after success
        nameEl.value = '';
        descEl.value = '';
        priceEl.value = '';
        imgEl.value = '';

        await loadProducts();
        alert("Product Published!");
    } catch (err) {
        console.error("Error adding product:", err);
        alert("Failed to publish product. Check server/API.");
    }
}

async function toggleStock(id, newStatus) {
    const token = localStorage.getItem('authToken');
    if (!token) { alert('Please sign in as admin'); return; }

    const res = await fetch(`${API_URL}/products/${id}/stock`, {
        method: 'PATCH',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({ inStock: !!newStatus })
    });
    if (!res.ok) throw new Error(`Toggle stock failed (${res.status})`);
    await loadProducts();
}

async function deleteProduct(id) {
    try {
        if (!confirm("Are you sure you want to delete this product?")) return;
        const res = await fetch(`${API_URL}/products/${id}`, { method: 'DELETE' });
        if (!res.ok) throw new Error(`Delete failed (${res.status})`);
        await loadProducts();
    } catch (err) {
        console.error("Error deleting product:", err);
        alert("Failed to delete product.");
    }
}