<template>
    <!-- ═══════════════════════════════════════════
       Hero
    ════════════════════════════════════════════ -->
    <header class="news-hero">
        <div>
            <p class="eat-label mb-2" style="color: var(--eat-secondary)">
                <i class="bi bi-newspaper me-2"></i>Lastest News
            </p>
            <h1 class="eat-h1 mb-2 eat-display">最新消息</h1>
            <div class="news-hero-sub">
                <div class="news-hero-line"></div>
                <span class="eat-body-muted mb-0">IL Manoscritto · News &amp; Stories</span>
                <div class="news-hero-line"></div>
            </div>
        </div>
    </header>

    <!-- ═══════════════════════════════════════════
       主體：側邊欄 + 文章列表
    ════════════════════════════════════════════ -->
    <main class="news-layout">
        <!-- 側邊分類導覽 -->
        <NewsSidebar
            :categories="categories"
            :selectedCategory="selectedCategory"
            @select="selectCategory"
        />

        <!-- 文章區塊 -->
        <section class="news-articles">
            <!-- 載入中 -->
            <div v-if="loading" class="news-state">
                <span class="news-state-text">載入中…</span>
            </div>

            <!-- 無文章 -->
            <div v-else-if="articles.length === 0" class="news-state">
                <span class="news-state-text">目前尚無消息，請稍後再回來。</span>
            </div>

            <template v-else>
                <template v-for="(article, index) in articles" :key="article.id">
                    <!-- 文章卡片：偶數左圖右文、奇數左文右圖 -->
                    <NewsCard :article="article" :index="index" />

                    <!-- 羽毛分隔線 -->
                    <div v-if="index < articles.length - 1" class="news-divider">
                        <svg width="200" height="20" viewBox="0 0 200 20" fill="none">
                            <path
                                d="M0 10C50 10 70 2 100 2C130 2 150 18 200 18"
                                stroke="#C9A96E"
                                stroke-width="0.5"
                                stroke-dasharray="4 4"
                            />
                        </svg>
                    </div>
                </template>

                <!-- 分頁 -->
                <div class="news-pagination">
                    <button
                        class="news-page-btn"
                        :disabled="page === 1"
                        @click="changePage(page - 1)"
                    >
                        <span class="">↼</span>
                    </button>
                    <div class="news-page-numbers">
                        <span
                            v-for="p in totalPages"
                            :key="p"
                            class="news-page-num"
                            :class="{ active: p === page }"
                            @click="changePage(p)"
                            >{{ p }}</span
                        >
                    </div>
                    <button
                        class="news-page-btn"
                        :disabled="page === totalPages"
                        @click="changePage(page + 1)"
                    >
                        <span class="">⇀</span>
                    </button>
                </div>
            </template>
        </section>
    </main>
    <BackToTop />
</template>

<script setup>
import { ref, watch, onMounted } from 'vue'
import apiFetch from '@/utils/apiFetch.js'
import NewsCard from '@/components/news/NewsCard.vue'
import NewsSidebar from '@/components/news/NewsSidebar.vue'
import BackToTop from '@/components/news/BackToTop.vue'

// ── 分類（對應後台 ArticleCategory 名稱）
const categories = [
    { id: null, label: '所有消息', en: 'ALL' },
    { id: '活動介紹', label: '活動介紹', en: 'EVENT' },
    { id: '餐廳公告', label: '餐廳公告', en: 'NOTICE' },
    { id: '季節限定', label: '季節限定', en: 'SEASON' },
    { id: '品牌故事', label: '品牌故事', en: 'STORY' },
    { id: '新品上市', label: '新品上市', en: 'NEW' },
    { id: '媒體報導', label: '媒體報導', en: 'MEDIA' },
]

// ── 狀態
const articles = ref([])
const loading = ref(false)
const page = ref(1)
const totalPages = ref(1)
const pageSize = 5
const selectedCategory = ref(null)

// ── 取得文章列表
async function fetchNews() {
    loading.value = true
    try {
        const params = new URLSearchParams({ page: page.value, pageSize })
        if (selectedCategory.value) {
            params.append('categoryName', selectedCategory.value)
        }
        const res = await apiFetch(`/News?${params}`)
        const data = await res.json()
        articles.value = data.data
        totalPages.value = data.totalPages // 後端給的才是正確的
    } catch (e) {
        console.error('載入最新消息失敗', e)
        articles.value = []
        totalPages.value = 1
    } finally {
        loading.value = false
    }
}

function changePage(p) {
    if (p < 1 || p > totalPages.value) return
    page.value = p
    window.scrollTo({ top: 0, behavior: 'smooth' })
}

function selectCategory(id) {
    selectedCategory.value = id
    page.value = 1
}

// ── 換頁或換分類時重新 fetch
watch([page, selectedCategory], fetchNews)
onMounted(fetchNews)
</script>

<style scoped>
/* ── Hero ─────────────────────────────────────────── */
.news-hero {
    padding: 3rem 2rem 3rem;
    text-align: center;
    background-color: var(--eat-surface-container);
    border-bottom: 1px solid var(--eat-outline-variant);
}
.news-hero-sub {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 1.25rem;
    opacity: 0.75;
}
.news-hero-line {
    flex: 0 0 3rem;
    height: 1px;
    background: var(--eat-secondary);
    opacity: 0.3;
}

/* ── Layout ───────────────────────────────────────── */
.news-layout {
    display: flex;
    flex-direction: column;
    gap: 3rem;
    max-width: 1440px;
    margin: 0 auto;
    padding: 4rem 2rem 8rem;
}
@media (min-width: 768px) {
    .news-layout {
        flex-direction: row;
        gap: 5rem;
        align-items: flex-start;
    }
}

/* ── Articles ─────────────────────────────────────── */
.news-articles {
    flex: 1;
    display: flex;
    flex-direction: column;
    gap: 3rem;
}

/* ── Divider ──────────────────────────────────────── */
.news-divider {
    display: flex;
    justify-content: center;
}

/* ── Pagination ───────────────────────────────────── */
.news-pagination {
    display: flex;
    justify-content: center;
    align-items: center;
    gap: 2rem;
    padding-top: 4rem;
}
.news-page-btn {
    width: 3rem;
    height: 3rem;
    border-radius: 50%;
    border: 1px solid var(--eat-outline-variant);
    background: transparent;
    color: var(--eat-secondary);
    font-size: 1.25rem;
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    transition: var(--eat-transition);
}
.news-page-btn:hover:not(:disabled) {
    background: var(--eat-primary);
    color: var(--eat-on-primary);
    border-color: var(--eat-primary);
}
.news-page-btn:disabled {
    opacity: 0.25;
    cursor: not-allowed;
}
.news-page-numbers {
    display: flex;
    gap: 1rem;
    font-family: var(--font-label);
    font-size: 1rem;
    letter-spacing: 0.12em;
}
.news-page-num {
    cursor: pointer;
    color: var(--eat-on-surface);
    opacity: 0.4;
    transition: all 0.2s ease;
    padding-bottom: 2px;
}
.news-page-num:hover,
.news-page-num.active {
    color: var(--eat-primary);
    opacity: 1;
}
.news-page-num.active {
    border-bottom: 1px solid var(--eat-primary);
}

/* ── 載入 / 空狀態 ────────────────────────────────── */
.news-state {
    padding: 6rem 0;
    text-align: center;
}
.news-state-text {
    font-family: var(--font-body);
    font-style: italic;
    color: var(--eat-on-surface-variant);
    opacity: 0.45;
}

/* ── RWD ──────────────────────────────────────────── */
@media (max-width: 767px) {
    .news-hero {
        padding: 5rem 1.5rem 3rem;
    }
    .news-layout {
        padding: 2.5rem 1.5rem 5rem;
    }
}
</style>
