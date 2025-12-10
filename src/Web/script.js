// ===== Configuration =====
const API_BASE_URL = window.location.origin;
const ENDPOINTS = {
    upload: `${API_BASE_URL}/api/v1/cnab-files`,
    stores: `${API_BASE_URL}/api/v1/stores`
};
const MAX_FILE_SIZE_MB = 5;
const MAX_FILE_SIZE_BYTES = MAX_FILE_SIZE_MB * 1024 * 1024;

// ===== State =====
const state = {
    page: 1,
    pageSize: 10,
    cpfFilter: '',
    totalPages: 1,
    totalItems: 0
};

// ===== DOM Elements =====
const elements = {
    uploadZone: document.getElementById('uploadZone'),
    fileInput: document.getElementById('fileInput'),
    progressContainer: document.getElementById('progressContainer'),
    progressFill: document.getElementById('progressFill'),
    progressPercentage: document.getElementById('progressPercentage'),
    progressStatus: document.getElementById('progressStatus'),
    successMessage: document.getElementById('successMessage'),
    refreshBtn: document.getElementById('refreshBtn'),
    loadingState: document.getElementById('loadingState'),
    emptyState: document.getElementById('emptyState'),
    storesList: document.getElementById('storesList'),
    // Filter elements
    cpfInput: document.getElementById('cpfInput'),
    searchBtn: document.getElementById('searchBtn'),
    clearBtn: document.getElementById('clearBtn'),
    pageSizeSelect: document.getElementById('pageSizeSelect'),
    // Pagination elements
    pagination: document.getElementById('pagination'),
    prevBtn: document.getElementById('prevBtn'),
    nextBtn: document.getElementById('nextBtn'),
    pageInfo: document.getElementById('pageInfo'),
    totalInfo: document.getElementById('totalInfo')
};

// ===== Initialization =====
document.addEventListener('DOMContentLoaded', () => {
    // Ensure success message is hidden on page load
    elements.successMessage.hidden = true;
    elements.progressContainer.hidden = true;

    initializeUploadZone();
    initializeRefreshButton();
    initializeFilters();
    initializePagination();
    loadStores();
});

// ===== Upload Zone =====
function initializeUploadZone() {
    const { uploadZone, fileInput } = elements;

    uploadZone.addEventListener('click', () => fileInput.click());

    fileInput.addEventListener('change', (e) => {
        const file = e.target.files[0];
        if (file) handleFileUpload(file);
    });

    uploadZone.addEventListener('dragover', (e) => {
        e.preventDefault();
        uploadZone.classList.add('drag-over');
    });

    uploadZone.addEventListener('dragleave', (e) => {
        e.preventDefault();
        uploadZone.classList.remove('drag-over');
    });

    uploadZone.addEventListener('drop', (e) => {
        e.preventDefault();
        uploadZone.classList.remove('drag-over');
        const file = e.dataTransfer.files[0];
        if (file) handleFileUpload(file);
    });
}

// ===== File Upload =====
function handleFileUpload(file) {
    const { progressContainer, progressFill, progressPercentage, progressStatus, successMessage, uploadZone } = elements;

    if (!file.name.match(/\.(txt|cnab)$/i)) {
        alert('Please select a valid CNAB file (.txt or .cnab)');
        return;
    }

    if (file.size > MAX_FILE_SIZE_BYTES) {
        alert(`File size (${formatBytes(file.size)}) exceeds the maximum allowed size of ${MAX_FILE_SIZE_MB}MB`);
        return;
    }

    successMessage.hidden = true;
    progressContainer.hidden = false;
    progressFill.style.width = '0%';
    progressPercentage.textContent = '0%';
    progressStatus.textContent = 'Preparing upload...';
    uploadZone.style.pointerEvents = 'none';

    const formData = new FormData();
    formData.append('file', file);

    const xhr = new XMLHttpRequest();

    xhr.upload.addEventListener('progress', (e) => {
        if (e.lengthComputable) {
            const percent = Math.round((e.loaded / e.total) * 100);
            if (percent < 100) {
                progressFill.style.width = `${percent}%`;
                progressPercentage.textContent = `${percent}%`;
                progressStatus.textContent = `Uploading ${formatBytes(e.loaded)} of ${formatBytes(e.total)}...`;
            }
        }
    });

    xhr.upload.addEventListener('loadend', () => {
        // Upload finished, now processing on server
        progressFill.style.width = '100%';
        progressFill.classList.add('indeterminate');
        progressPercentage.textContent = '';
        progressStatus.textContent = 'Processing file on server...';
    });

    xhr.addEventListener('load', () => {
        uploadZone.style.pointerEvents = 'auto';
        progressFill.classList.remove('indeterminate');
        if (xhr.status >= 200 && xhr.status < 300) {
            progressContainer.hidden = true;
            successMessage.hidden = false;
            setTimeout(() => {
                state.page = 1;
                loadStores();
            }, 1000);
        } else if (xhr.status === 413) {
            progressStatus.textContent = `Error: File too large. Maximum allowed is ${MAX_FILE_SIZE_MB}MB`;
            progressFill.style.background = 'var(--danger)';
        } else {
            progressStatus.textContent = `Error: ${xhr.statusText || 'Upload failed'}`;
            progressFill.style.background = 'var(--danger)';
        }
    });

    xhr.addEventListener('error', () => {
        uploadZone.style.pointerEvents = 'auto';
        progressStatus.textContent = 'Network error. Please try again.';
        progressFill.style.background = 'var(--danger)';
    });

    xhr.open('POST', ENDPOINTS.upload);
    xhr.setRequestHeader('X-Client-Id', window.CLIENT_ID);
    xhr.send(formData);
}

// ===== Refresh Button =====
function initializeRefreshButton() {
    elements.refreshBtn.addEventListener('click', () => loadStores());
}

// ===== Filters =====
function initializeFilters() {
    const { cpfInput, searchBtn, clearBtn, pageSizeSelect } = elements;

    searchBtn.addEventListener('click', () => {
        state.cpfFilter = cpfInput.value.replace(/\D/g, '');
        state.page = 1;
        loadStores();
    });

    cpfInput.addEventListener('keypress', (e) => {
        if (e.key === 'Enter') {
            state.cpfFilter = cpfInput.value.replace(/\D/g, '');
            state.page = 1;
            loadStores();
        }
    });

    clearBtn.addEventListener('click', () => {
        cpfInput.value = '';
        state.cpfFilter = '';
        state.page = 1;
        loadStores();
    });

    pageSizeSelect.addEventListener('change', (e) => {
        state.pageSize = parseInt(e.target.value, 10);
        state.page = 1;
        loadStores();
    });
}

// ===== Pagination =====
function initializePagination() {
    const { prevBtn, nextBtn } = elements;

    prevBtn.addEventListener('click', () => {
        if (state.page > 1) {
            state.page--;
            loadStores();
        }
    });

    nextBtn.addEventListener('click', () => {
        if (state.page < state.totalPages) {
            state.page++;
            loadStores();
        }
    });
}

function updatePaginationUI(data) {
    const { pagination, prevBtn, nextBtn, pageInfo, totalInfo } = elements;

    state.totalPages = data.totalPages;
    state.totalItems = data.totalItems;

    pagination.hidden = data.totalItems === 0;
    prevBtn.disabled = !data.hasPreviousPage;
    nextBtn.disabled = !data.hasNextPage;
    pageInfo.textContent = `Page ${data.page} of ${data.totalPages}`;
    totalInfo.textContent = `(${data.totalItems} stores)`;
}

// ===== Load Stores =====
async function loadStores() {
    const { refreshBtn, loadingState, emptyState, storesList } = elements;

    refreshBtn.classList.add('spinning');
    loadingState.hidden = false;
    emptyState.hidden = true;
    storesList.innerHTML = '';
    elements.pagination.hidden = true;

    try {
        const params = new URLSearchParams({
            page: state.page,
            pageSize: state.pageSize
        });
        if (state.cpfFilter) {
            params.append('cpf', state.cpfFilter);
        }

        const response = await fetch(`${ENDPOINTS.stores}?${params}`);
        if (!response.ok) throw new Error(`HTTP ${response.status}`);

        const data = await response.json();

        loadingState.hidden = true;
        refreshBtn.classList.remove('spinning');

        if (!data.stores || data.stores.length === 0) {
            emptyState.hidden = false;
            elements.pagination.hidden = true;
            return;
        }

        renderStores(data.stores);
        updatePaginationUI(data);

    } catch (error) {
        console.error('Error loading stores:', error);
        loadingState.hidden = true;
        refreshBtn.classList.remove('spinning');
        emptyState.hidden = false;
    }
}

// ===== Render Stores =====
function renderStores(stores) {
    const { storesList } = elements;
    storesList.innerHTML = '';
    stores.forEach(store => storesList.appendChild(createStoreCard(store)));
}

function createStoreCard(store) {
    const card = document.createElement('div');
    card.className = 'store-card';

    const isPositive = store.balance >= 0;
    const balanceClass = isPositive ? 'positive' : 'negative';

    card.innerHTML = `
        <div class="store-header">
            <div class="store-info">
                <div class="store-name">${escapeHtml(store.name)}</div>
                <div class="store-owner">Owner: ${escapeHtml(store.ownerName)}</div>
            </div>
            <div class="store-balance">
                <div class="balance-label">Balance</div>
                <div class="balance-value ${balanceClass}">${formatCurrency(store.balance)}</div>
            </div>
            <svg class="expand-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <polyline points="6 9 12 15 18 9"/>
            </svg>
        </div>
        <div class="transactions-container">
            <div class="transactions-list">${renderTransactions(store.transactions)}</div>
        </div>
    `;

    card.querySelector('.store-header').addEventListener('click', () => {
        card.classList.toggle('expanded');
    });

    return card;
}

function renderTransactions(transactions) {
    if (!transactions || transactions.length === 0) {
        return '<p style="padding: 1rem; color: var(--text-muted);">No transactions</p>';
    }

    // Sort transactions by date descending (newest first)
    const sortedTransactions = [...transactions].sort((a, b) => {
        return new Date(b.date) - new Date(a.date);
    });

    return sortedTransactions.map(t => {
        const isIncome = t.sign === '+';
        const typeClass = isIncome ? 'income' : 'expense';

        return `
            <div class="transaction-item">
                <span class="transaction-type ${typeClass}">${escapeHtml(t.type)}</span>
                <div class="transaction-details">
                    <span class="transaction-date">${formatDate(t.date)}</span>
                    <span class="transaction-meta">Card: ${escapeHtml(t.cardNumber)} | CPF: ${formatCpf(t.cpf)}</span>
                </div>
                <span class="transaction-amount ${typeClass}">${t.sign} ${formatCurrency(Math.abs(t.amount))}</span>
            </div>
        `;
    }).join('');
}

// ===== Utility Functions =====
function formatBytes(bytes) {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
}

function formatCurrency(value) {
    return new Intl.NumberFormat('pt-BR', {
        style: 'currency',
        currency: 'BRL'
    }).format(value);
}

function formatDate(dateString) {
    const date = new Date(dateString);
    return new Intl.DateTimeFormat('pt-BR', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    }).format(date);
}

function formatCpf(cpf) {
    if (!cpf || cpf.length !== 11) return cpf;
    return `${cpf.slice(0, 3)}.${cpf.slice(3, 6)}.${cpf.slice(6, 9)}-${cpf.slice(9)}`;
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}
