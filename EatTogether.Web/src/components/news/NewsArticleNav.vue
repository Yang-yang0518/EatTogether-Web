<template>
    <nav class="detail-nav">
        <RouterLink
            v-if="prevArticle"
            :to="{ name: 'NewsDetail', params: { id: prevArticle.id } }"
            class="detail-nav-link"
        >
            <span class="detail-nav-arrow">←</span>
            <div class="detail-nav-info">
                <span class="detail-nav-label">上一篇</span>
                <span class="detail-nav-ttl">{{ prevArticle.title }}</span>
            </div>
        </RouterLink>
        <div v-else></div>

        <RouterLink
            v-if="nextArticle"
            :to="{ name: 'NewsDetail', params: { id: nextArticle.id } }"
            class="detail-nav-link detail-nav-link--right"
        >
            <div class="detail-nav-info detail-nav-info--right">
                <span class="detail-nav-label">下一篇</span>
                <span class="detail-nav-ttl">{{ nextArticle.title }}</span>
            </div>
            <span class="detail-nav-arrow">→</span>
        </RouterLink>
        <div v-else></div>
    </nav>
</template>

<script setup>
import { RouterLink } from 'vue-router'

defineProps({
    prevArticle: {
        type: Object,
        default: null,
    },
    nextArticle: {
        type: Object,
        default: null,
    },
})
</script>

<style scoped>
/* ── 上下篇導覽 ───────────────────────────────────── */
.detail-nav {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    gap: 1.5rem;
    border-top: 1px solid var(--eat-outline-variant);
    padding-top: 2rem;
    margin-bottom: 3rem;
}
.detail-nav-link {
    display: flex;
    align-items: center;
    gap: 0.75rem;
    text-decoration: none;
    color: var(--eat-on-surface-variant);
    opacity: 0.7;
    transition: all 0.25s ease;
    max-width: 45%;
}
.detail-nav-link:hover {
    opacity: 1;
    color: var(--eat-primary);
}
.detail-nav-link--right {
    justify-content: flex-end;
}
.detail-nav-arrow {
    font-size: 1.3rem;
    flex-shrink: 0;
    color: var(--eat-secondary);
}
.detail-nav-info {
    display: flex;
    flex-direction: column;
    gap: 0.2rem;
}
.detail-nav-info--right {
    text-align: right;
}

.detail-nav-label {
    font-family: var(--font-label);
    font-size: 0.9rem;
    letter-spacing: 0.25em;
    text-transform: uppercase;
    opacity: 0.7;
}
.detail-nav-ttl {
    font-family: var(--font-headline);
    font-style: italic;
    font-size: 1.25rem;
    line-height: 1.4;
    display: -webkit-box;
    -webkit-line-clamp: 2;
    line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
}

/* ── RWD ──────────────────────────────────────────── */
@media (max-width: 767px) {
    .detail-nav {
        flex-direction: column;
        gap: 1.5rem;
    }
    .detail-nav-link {
        max-width: 100%;
    }
    .detail-nav-link--right {
        justify-content: flex-start;
    }
    .detail-nav-info--right {
        text-align: left;
    }
}
</style>
