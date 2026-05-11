<template>
    <aside class="news-sidebar">
        <div class="news-sidebar-inner">
            <div class="news-sidebar-heading">
                <h2 class="news-sidebar-title">Il Manoscritto</h2>
                <p class="news-sidebar-sub">News &amp; Stories</p>
            </div>
            <nav class="news-sidebar-nav">
                <a
                    v-for="cat in categories"
                    :key="cat.id ?? 'all'"
                    class="news-sidebar-link"
                    :class="{ active: selectedCategory === cat.id }"
                    href="#"
                    @click.prevent="$emit('select', cat.id)"
                >
                    <span class="news-sidebar-label">{{ cat.label }}</span>
                    <span class="news-sidebar-en">{{ cat.en }}</span>
                </a>
            </nav>
        </div>
    </aside>
</template>

<script setup>
defineProps({
    categories: {
        type: Array,
        required: true,
    },
    selectedCategory: {
        type: String,
        default: null,
    },
})

defineEmits(['select'])
</script>

<style scoped>
/* ── 桌機：直排側邊欄 ─────────────────────────────── */
.news-sidebar {
    width: 100%;
    flex-shrink: 0;
}
@media (min-width: 768px) {
    .news-sidebar {
        width: 15rem;
    }
}

.news-sidebar-inner {
    position: sticky;
    top: 6rem;
    display: flex;
    flex-direction: column;
    gap: 2rem;
}

/* 標題區：手機隱藏，桌機才顯示 */
.news-sidebar-heading {
    display: none;
}
@media (min-width: 768px) {
    .news-sidebar-heading {
        display: block;
    }
}

.news-sidebar-title {
    font-family: var(--font-headline);
    font-style: italic;
    font-size: 1.9rem;
    color: var(--eat-primary);
    margin-bottom: 0;
}
.news-sidebar-sub {
    font-family: var(--font-label);
    font-size: 0.9rem;
    letter-spacing: 0.35em;
    text-transform: uppercase;
    color: var(--eat-on-surface-variant);
    opacity: 0.7;
    margin-top: 0.3rem;
}

/* ── 分類導覽：手機橫向捲動、桌機直排 ────────────── */
.news-sidebar-nav {
    display: flex;
    gap: 0.5rem;

    /* 手機：橫向捲動 */
    overflow-x: auto;
    scrollbar-width: none;
}
.news-sidebar-nav::-webkit-scrollbar {
    display: none;
}

@media (min-width: 768px) {
    .news-sidebar-nav {
        /* 桌機：直排，關閉橫向捲動 */
        flex-direction: column;
        gap: 1.25rem;
        overflow-x: visible;
    }
}

/* ── 分類連結：手機膠囊、桌機文字列 ──────────────── */
.news-sidebar-link {
    flex-shrink: 0;
    white-space: nowrap;
    text-decoration: none;
    transition: all 0.25s ease;

    /* 手機：膠囊樣式 */
    padding: 0.4rem 1rem;
    border-radius: 999px;
    border: 1px solid rgba(201, 169, 110, 0.3);
    color: var(--eat-secondary);
    font-family: var(--font-label);
    font-size: 0.75rem;
    letter-spacing: 0.15em;
    opacity: 0.6;
}
.news-sidebar-link:hover {
    background: rgba(201, 169, 110, 0.1);
    opacity: 1;
}
.news-sidebar-link.active {
    background: var(--eat-secondary);
    color: var(--eat-surface-lowest);
    border-color: var(--eat-secondary);
    opacity: 1;
    font-weight: 600;
}

@media (min-width: 768px) {
    /* 桌機：回到原本文字列樣式 */
    .news-sidebar-link {
        display: flex;
        align-items: center;
        gap: 0.75rem;
        padding: 0;
        border-radius: 0;
        border: none;
        font-size: 1.25rem;
        letter-spacing: 0.08em;
        opacity: 0.7;
    }
    .news-sidebar-link:hover,
    .news-sidebar-link.active {
        background: transparent;
        color: var(--eat-primary);
        opacity: 1;
        transform: translateX(6px);
    }
    .news-sidebar-link.active {
        font-weight: 600;
    }
}

/* 桌機才顯示 label 和 en 的樣式 */
.news-sidebar-label {
    font-style: italic;
}
.news-sidebar-en {
    font-family: var(--font-headline);
    font-style: italic;
    font-size: 1rem;
    opacity: 0.7;
    letter-spacing: 0.1em;
    min-width: 3.5rem;
    margin-left: 0.5rem;
}

/* 手機隱藏 en 英文標籤，只顯示中文 */
@media (max-width: 767px) {
    .news-sidebar-en {
        display: none;
    }
}
</style>
