<template>
    <article
        class="news-card"
        :class="[
            index % 2 === 0 ? 'news-card--normal' : 'news-card--reverse',
            article.isPinned ? 'news-card--pinned' : '',
        ]"
    >
        <!-- 圖片欄 -->
        <div class="news-card-img-wrap">
            <RouterLink :to="{ name: 'NewsDetail', params: { id: article.id } }">
                <img
                    v-if="article.coverImageUrl"
                    :src="article.coverImageUrl"
                    :alt="article.title"
                    class="news-card-img"
                />
                <div v-else class="news-card-img-placeholder">
                    <span style="opacity: 0.3; letter-spacing: 0.2em">NO IMAGE</span>
                </div>
            </RouterLink>
        </div>

        <!-- 文字欄 -->
        <div class="news-card-body" :class="index % 2 !== 0 ? 'news-card-body--right' : ''">
            <!-- 置頂角標 -->
            <div
                v-if="article.isPinned"
                class="news-card-pin"
                :class="index % 2 !== 0 ? 'news-card-pin--left' : 'news-card-pin--right'"
            >
                <i class="bi bi-pin-fill"></i>
            </div>
            <div class="news-card-date-bg">
                {{ formatDateBg(article.publishDate) }}
            </div>
            <span class="news-card-category">{{ article.categoryName }}</span>
            <RouterLink :to="{ name: 'NewsDetail', params: { id: article.id } }">
                <h3 class="eat-h3 news-card-title">{{ article.title }}</h3>
            </RouterLink>
            <!-- eslint-disable-next-line vue/no-v-html -->
            <p class="eat-body news-card-summary" v-html="article.summary"></p>
            <div class="news-card-meta">
                <i class="bi bi-eye"></i>
                <span>{{ article.viewCount }}</span>
            </div>
            <RouterLink
                :to="{
                    name: 'NewsDetail',
                    params: { id: article.id },
                }"
                class="news-card-link"
                :class="index % 2 !== 0 ? 'news-card-link--reverse' : ''"
            >
                <template v-if="index % 2 !== 0"> 閱讀全文 → </template>
                <template v-else> ← 閱讀全文 </template>
            </RouterLink>
        </div>
    </article>
</template>

<script setup>
import { RouterLink } from 'vue-router'

defineProps({
    article: {
        type: Object,
        required: true,
    },
    index: {
        type: Number,
        required: true,
    },
})

function formatDateBg(dateStr) {
    if (!dateStr) return ''
    const d = new Date(dateStr)
    const y = d.getFullYear()
    const m = String(d.getMonth() + 1).padStart(2, '0')
    const day = String(d.getDate()).padStart(2, '0')
    return `${y}.${m}.${day}`
}
</script>

<style scoped>
/* ── Card ─────────────────────────────────────────── */
.news-card {
    display: flex;
    flex-direction: column;
    align-items: center;
}
@media (min-width: 768px) {
    .news-card--normal {
        flex-direction: row;
    }
    .news-card--reverse {
        flex-direction: row-reverse;
    }
}

/* ── 圖片欄 ───────────────────────────────────────── */
.news-card-img-wrap {
    width: 100%;
    overflow: hidden;
    border-radius: var(--eat-radius);
    box-shadow: 0 25px 50px -12px rgba(24, 11, 6, 0.5);
    flex-shrink: 0;
    align-self: stretch;
    aspect-ratio: 4 / 3;
}
@media (min-width: 768px) {
    .news-card-img-wrap {
        width: 40%;
    }
}

.news-card-img {
    width: 100%;
    height: 100%;
    min-height: 280px;
    object-fit: cover;
    display: block;
    transition: transform 0.7s cubic-bezier(0.4, 0, 0.2, 1);
}
.news-card:hover .news-card-img {
    transform: scale(1.05);
}

.news-card-img-placeholder {
    width: 100%;
    height: 100%;
    min-height: 280px;
    background: var(--eat-surface-high);
    display: flex;
    align-items: center;
    justify-content: center;
    color: var(--eat-on-surface-variant);
    opacity: 0.25;
}

/* ── 文字欄 ───────────────────────────────────────── */
.news-card-body {
    width: 100%;
    padding: 2rem;
    padding-top: 4rem;
    background: var(--eat-surface-high);
    border-radius: var(--eat-radius);
    position: relative;
}
.news-card-body--right {
    text-align: right;
}

/* ── 置頂角標 ─────────────────────────────────────── */
.news-card-pin {
    position: absolute;
    top: 1rem;
    z-index: 1;
    font-size: 1.2rem;
    color: var(--eat-secondary);
}
.news-card-pin--right {
    right: 1rem;
    rotate: 45deg;
}
.news-card-pin--left {
    left: 1rem;
    rotate: -45deg;
}

/* ── 日期浮水印 ───────────────────────────────────── */
.news-card-date-bg {
    position: absolute;
    top: 1rem;
    left: 2rem;
    font-family: var(--font-headline);
    font-size: 2rem;
    font-style: italic;
    color: var(--eat-on-surface);
    opacity: 0.4;
    pointer-events: none;
    user-select: none;
}
.news-card-body--right .news-card-date-bg {
    left: auto;
    right: 2rem;
}

/* ── 分類 / 標題 / 摘要 ───────────────────────────── */
.news-card-category {
    display: block;
    font-family: var(--font-label);
    font-size: 1rem;
    letter-spacing: 0.3em;
    text-transform: uppercase;
    color: var(--eat-secondary);
    margin-bottom: 1rem;
}
.news-card-title {
    font-size: 1.9rem;
    line-height: 1.35;
    margin-bottom: 1.25rem;
    text-align: left;
}
a .news-card-title {
    color: var(--eat-primary);
    text-decoration: none;
    transition: color 0.2s ease;
}
a:hover .news-card-title {
    color: var(--eat-on-surface);
    text-decoration: underline;
    text-underline-offset: 4px;
}
.news-card-summary {
    opacity: 0.75;
    font-style: italic;
    font-size: 1.1rem;
    margin-bottom: 1.25rem;
    display: -webkit-box;
    -webkit-line-clamp: 3;
    line-clamp: 3;
    -webkit-box-orient: vertical;
    overflow: hidden;
    text-align: left;
}

/* ── Meta（瀏覽數）────────────────────────────────── */
.news-card-meta {
    display: flex;
    align-items: center;
    gap: 0.3rem;
    font-family: var(--font-label);
    font-size: 0.9rem;
    color: var(--eat-on-surface-variant);
    opacity: 0.7;
    margin-bottom: 1.75rem;
}
.news-card-body--right .news-card-meta {
    justify-content: flex-end;
}

/* ── 閱讀全文連結 ─────────────────────────────────── */
.news-card-link {
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
    font-family: var(--font-label);
    font-size: 0.9rem;
    letter-spacing: 0.2em;
    text-transform: uppercase;
    color: var(--eat-primary);
    text-decoration: none;
    transition:
        transform 0.3s ease,
        gap 0.3s ease;
}
.news-card-link:hover {
    gap: 0.85rem;
    transform: translateX(6px);
}
.news-card-link--reverse {
    flex-direction: row-reverse;
}
.news-card-link--reverse:hover {
    transform: translateX(-6px);
}

/* ── 接縫圓角（normal / reverse）────────────────────── */
.news-card--normal .news-card-img-wrap {
    border-radius: var(--eat-radius) 0 0 var(--eat-radius);
}
.news-card--normal .news-card-body {
    border-radius: 0 var(--eat-radius) var(--eat-radius) 0;
}
.news-card--reverse .news-card-img-wrap {
    border-radius: 0 var(--eat-radius) var(--eat-radius) 0;
}
.news-card--reverse .news-card-body {
    border-radius: var(--eat-radius) 0 0 var(--eat-radius);
}
</style>
