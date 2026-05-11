<template>
    <header class="detail-intro">
        <div class="detail-intro-img">
            <img v-if="article.coverImageUrl" :src="article.coverImageUrl" :alt="article.title" />
            <div v-else class="detail-intro-img-placeholder"></div>
        </div>
        <div class="detail-intro-text">
            <div class="detail-pin-row">
                <i v-if="article.isPinned" class="bi bi-pin-fill detail-pin-icon"></i>
            </div>
            <div class="detail-title-row">
                <h1 class="detail-title eat-h1">{{ article.title }}</h1>
            </div>
            <span class="detail-eyebrow">{{ article.categoryName }}</span>
            <div class="detail-meta-row">
                <span>{{ formatDate(article.publishDate) }}</span>
                <span class="detail-sep">·</span>
                <span>{{ article.viewCount }} 次閱覽</span>
            </div>
        </div>
    </header>
</template>

<script setup>
defineProps({
    article: {
        type: Object,
        required: true,
    },
})

function formatDate(dateStr) {
    if (!dateStr) return ''
    const d = new Date(dateStr)
    return `${d.getFullYear()}.${String(d.getMonth() + 1).padStart(2, '0')}.${String(d.getDate()).padStart(2, '0')}`
}
</script>

<style scoped>
/* ── Intro 滿版橫列 ───────────────────────────────── */
.detail-intro {
    display: flex;
    flex-direction: row;
    width: 100%;
    height: 320px;
}

/* ── 圖片欄 ───────────────────────────────────────── */
.detail-intro-img {
    width: 30%;
    height: 100%;
    overflow: hidden;
    flex-shrink: 0;
}
.detail-intro-img img {
    width: 100%;
    height: 100%;
    object-fit: cover;
    object-position: center;
    display: block;
    transition: transform 0.7s cubic-bezier(0.4, 0, 0.2, 1);
}
.detail-intro:hover .detail-intro-img img {
    transform: scale(1.04);
}
.detail-intro-img-placeholder {
    width: 100%;
    height: 100%;
    background: var(--eat-surface-high);
}

/* ── 文字欄 ───────────────────────────────────────── */
.detail-intro-text {
    flex: 1;
    background: var(--eat-surface-high);
    padding: 2.5rem 3rem 2rem;
    display: flex;
    flex-direction: column;
    justify-content: center;
    gap: 0.75rem;
    position: relative;
}

.detail-pin-row {
    position: relative;
}

/* ── 標題列 ───────────────────────────────────────── */
.detail-title-row {
    position: relative;
}
.detail-title {
    margin: 0;
}
.detail-pin-icon {
    position: absolute;
    top: -1rem;
    left: 0.5rem;
    font-size: 1rem;
    color: var(--eat-secondary);
    opacity: 0.8;
    rotate: 45deg;
    line-height: 1.2;
}

/* ── 分類 / Meta ──────────────────────────────────── */
.detail-eyebrow {
    display: block;
    font-family: var(--font-label);
    font-size: 0.9rem;
    letter-spacing: 0.4em;
    text-transform: uppercase;
    color: var(--eat-secondary);
    margin-left: 0.6rem;
    line-height: 2;
    padding-top: 1em;
}
.detail-meta-row {
    position: relative;
    display: flex;
    align-items: center;
    gap: 0.5rem;
    font-family: var(--font-label);
    font-size: 0.9rem;
    color: var(--eat-on-surface-variant);
    opacity: 0.8;
    margin-left: 0.6rem;
    line-height: 1;
}
.detail-sep {
    opacity: 0.4;
}

/* ── RWD ──────────────────────────────────────────── */
@media (max-width: 767px) {
    .detail-intro {
        flex-direction: column;
        height: auto;
    }
    .detail-intro-img {
        width: 100%;
        height: 260px;
    }
}
</style>
