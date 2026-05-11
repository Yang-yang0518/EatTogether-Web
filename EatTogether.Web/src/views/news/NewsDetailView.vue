<template>
    <div class="detail-root">
        <div v-if="loading" class="detail-state" style="padding-top: 8rem">
            <span class="detail-state-text">載入中…</span>
        </div>

        <div v-else-if="error" class="detail-state" style="padding-top: 8rem">
            <span class="detail-state-text">文章載入失敗，請稍後再試。</span>
        </div>

        <template v-else>
            <!-- Intro：圖左文右 -->
            <NewsIntroHeader :article="article" />

            <!-- 麵包屑 -->
            <nav class="detail-breadcrumb">
                <RouterLink :to="{ name: 'Home' }" class="detail-bc-link">Home</RouterLink>
                <span class="detail-bc-sep">/</span>
                <RouterLink :to="{ name: 'NewsList' }" class="detail-bc-link">News</RouterLink>
                <span class="detail-bc-sep">/</span>
                <span class="detail-bc-current">{{ article.title }}</span>
            </nav>

            <!-- 主體 -->
            <main class="detail-main">
                <!-- eslint-disable-next-line vue/no-v-html -->
                <article class="detail-body" v-html="article.description"></article>
                <!-- 這裡保留是文章非純文字會含有html標籤，使用v-html呈現。以白名單方式預防XSS攻擊，後端會過濾掉不安全的標籤與屬性。 -->

                <div class="detail-divider">
                    <svg width="200" height="20" viewBox="0 0 200 20" fill="none">
                        <path
                            d="M0 10C50 10 70 2 100 2C130 2 150 18 200 18"
                            stroke="#C9A96E"
                            stroke-width="0.5"
                            stroke-dasharray="4 4"
                        />
                    </svg>
                </div>

                <div v-if="article.categoryName" class="detail-tags">
                    <span class="detail-tag"># {{ article.categoryName }}</span>
                </div>

                <NewsArticleNav :prevArticle="prevArticle" :nextArticle="nextArticle" />

                <div class="detail-back-wrap">
                    <RouterLink :to="{ name: 'NewsList' }" class="btn-eat-secondary btn-eat-md"
                        >回列表頁</RouterLink
                    >
                </div>
            </main>
        </template>
        <BackToTop />
    </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'
import { useRoute, useRouter, RouterLink } from 'vue-router'
import apiFetch from '@/utils/apiFetch.js'
import NewsIntroHeader from '@/components/news/NewsIntroHeader.vue'
import NewsArticleNav from '@/components/news/NewsArticleNav.vue'
import { useToast } from '@/composables/useToast.js'
import BackToTop from '@/components/news/BackToTop.vue'

const route = useRoute()
const router = useRouter()

const article = ref({
    title: '',
    categoryName: '',
    description: '',
    coverImageUrl: null,
    publishDate: null,
    status: '',
    viewCount: 0,
})
const prevArticle = ref(null)
const nextArticle = ref(null)
const loading = ref(false)
const error = ref(false)
const { show } = useToast()

// 取得或產生訪客 ID
function getVisitorId() {
    let id = localStorage.getItem('visitor_id')
    if (!id) {
        id = crypto.randomUUID()
        localStorage.setItem('visitor_id', id)
    }
    return id
}

// 5 分鐘內同一篇不重複發請求
function shouldIncrementView(id) {
    const key = `viewed_article_${id}`
    const last = localStorage.getItem(key)
    const now = Date.now()
    const cooldown = 5 * 60 * 1000

    if (last && now - parseInt(last) < cooldown) return false

    localStorage.setItem(key, now.toString())
    return true
}

async function fetchDetail(id) {
    loading.value = true
    error.value = false
    try {
        const res = await apiFetch(`/News/${id}`)
        if (!res.ok) {
            if (res.status === 404) {
                show('此文章已下架或不存在')
                router.replace({ name: 'NewsList' })
                return
            }
            error.value = true
            return
        }
        const data = await res.json()

        article.value = data.article
        prevArticle.value = data.prev ?? null
        nextArticle.value = data.next ?? null

        // 前端先擋一次，通過才發點閱紀錄請求
        if (shouldIncrementView(id)) {
            apiFetch(`/News/${id}/view`, {
                method: 'POST',
                headers: { 'X-Visitor-Id': getVisitorId() },
            }).catch(() => {})
        }
    } catch (e) {
        console.error('載入文章失敗', e)
        error.value = true
    } finally {
        loading.value = false
    }
}

watch(
    () => route.params.id,
    (id) => {
        if (id) fetchDetail(id)
    }
)

onMounted(() => {
    fetchDetail(route.params.id)
})
</script>

<style scoped>
.detail-root {
    background-color: var(--eat-surface-lowest);
    min-height: 100vh;
    padding-bottom: 6rem;
}

/* ── 麵包屑 ───────────────────────────────────────── */
.detail-breadcrumb {
    max-width: 1100px;
    margin: 0 auto;
    padding: 1.25rem 2rem;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 0.5rem;
    font-family: var(--font-label);
    font-size: 0.9rem;
    letter-spacing: 0.15em;
    text-transform: uppercase;
    color: var(--eat-on-surface-variant);
    opacity: 0.7;
    flex-wrap: wrap;
}
.detail-bc-link {
    color: inherit;
    text-decoration: none;
    transition: color 0.2s;
}
.detail-bc-link:hover {
    color: var(--eat-primary);
    opacity: 1;
}
.detail-bc-sep {
    opacity: 0.3;
}
.detail-bc-current {
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    max-width: 240px;
}

/* ── 主體 ─────────────────────────────────────────── */
.detail-main {
    max-width: 1080px;
    margin: 0 auto;
    padding: 2.5rem 2rem 4rem;
}

/* ── 內文排版 ──────────────────────────────────────── */
.detail-body {
    font-family: var(--font-body);
    font-size: 1.4rem;
    line-height: 2.2;
    color: var(--eat-on-surface);
    opacity: 0.85;
}
.detail-body :deep(p) {
    margin-bottom: 3rem;
}
.detail-body :deep(strong) {
    font-weight: 600;
}
.detail-body :deep(em) {
    font-style: italic;
    color: var(--eat-secondary);
}
.detail-body :deep(u) {
    text-underline-offset: 4px;
}
.detail-body :deep(a) {
    color: var(--eat-primary);
    text-decoration: underline;
    text-underline-offset: 3px;
    transition: opacity 0.2s;
}
.detail-body :deep(a:hover) {
    opacity: 0.7;
}

/* Quill 字級 */
.detail-body :deep(.ql-size-small),
.ql-size-small {
    font-size: 0.85rem;
}
.detail-body :deep(.ql-size-large),
.ql-size-large {
    font-size: 1.3rem;
}
.detail-body :deep(.ql-size-huge),
.ql-size-huge {
    font-size: 1.6rem;
}

/* ── 分隔線 ───────────────────────────────────────── */
.detail-divider {
    display: flex;
    justify-content: center;
    margin: 3rem 0 2rem;
    opacity: 0.35;
}

/* ── 標籤 ─────────────────────────────────────────── */
.detail-tags {
    display: flex;
    flex-wrap: wrap;
    gap: 0.5rem;
    margin-bottom: 3rem;
}
.detail-tag {
    font-family: var(--font-label);
    font-size: 0.9rem;
    letter-spacing: 0.2em;
    padding: 0.3rem 0.9rem;
    background: var(--eat-surface-high);
    color: var(--eat-on-surface-variant);
    border-radius: var(--eat-radius);
    transition:
        background 0.2s,
        color 0.2s;
}
.detail-tag:hover {
    background: var(--eat-surface-highest);
    color: var(--eat-secondary);
}

/* ── 回列表按鈕 ────────────────────────────────────── */
.detail-back-wrap {
    display: flex;
    justify-content: center;
}

/* ── 載入 / 錯誤狀態 ──────────────────────────────── */
.detail-state {
    padding: 6rem 0;
    text-align: center;
}
.detail-state-text {
    font-family: var(--font-body);
    font-style: italic;
    color: var(--eat-on-surface-variant);
    opacity: 0.45;
}

/* ── RWD ──────────────────────────────────────────── */
@media (max-width: 767px) {
    .detail-main {
        padding: 1.5rem 1.25rem 4rem;
    }
    .detail-body {
        font-size: 1rem;
    }
    .detail-bc-current {
        max-width: 160px;
    }
}
</style>
