<!-- 留言列表 + 送出表單 -->
<template>
    <div class="review-section">
        <div class="modal-section-label">留言區</div>

        <div v-if="loading" class="review-loading">
            <span class="ingredient-spinner"></span> 載入留言中...
        </div>
        <div v-else-if="reviews.length" class="review-list">
            <div v-for="r in displayed" :key="r.id" class="review-item">
                <div class="review-meta">
                    <span class="review-nickname">{{ r.nickname }}</span>
                    <span class="review-time">{{ relativeTime(r.createdAt) }}</span>
                </div>
                <p class="review-content">{{ r.content }}</p>
            </div>
            <button
                v-if="reviews.length > 5 && !showAll"
                class="review-more-btn"
                @click="showAll = true"
            >
                查看更多（共 {{ reviews.length }} 則）
            </button>
        </div>
        <p v-else class="review-empty">尚無留言，成為第一個留言的人！</p>

        <div class="rating-divider"></div>

        <div v-if="isLoggedIn" class="review-form">
            <textarea
                v-model="content"
                placeholder="留下您的感想…（最多 200 字）"
                maxlength="200"
                class="review-textarea"
                rows="3"
            ></textarea>
            <div class="review-form-footer">
                <span class="review-char-count">{{ content.length }} / 200</span>
                <button class="review-submit-btn" :disabled="!content.trim()" @click="submit">
                    送出留言
                </button>
            </div>
        </div>
        <div v-else class="login-hint">
            <span class="login-hint-text">登入後即可留言</span>
            <button class="login-hint-btn" @click="emit('login')">立即登入</button>
        </div>
    </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import apiFetch from '@/utils/apiFetch.js'
import { useToast } from '@/composables/useToast.js'

const props = defineProps({
    dishId: { type: Number, required: true },
    isLoggedIn: { type: Boolean, default: false },
    memberName: { type: String, default: '' },
})

const emit = defineEmits(['login'])

const { show } = useToast()

const reviews = ref([])
const loading = ref(false)
const showAll = ref(false)
const content = ref('')

const displayed = computed(() => (showAll.value ? reviews.value : reviews.value.slice(0, 5)))

const relativeTime = (dateStr) => {
    const diff = Date.now() - new Date(dateStr).getTime()
    const minutes = Math.floor(diff / 60000)
    if (minutes < 1) return '剛剛'
    if (minutes < 60) return `${minutes} 分鐘前`
    const hours = Math.floor(minutes / 60)
    if (hours < 24) return `${hours} 小時前`
    return `${Math.floor(hours / 24)} 天前`
}

const load = async () => {
    loading.value = true
    reviews.value = []
    showAll.value = false
    try {
        const res = await apiFetch(`/reviews/${props.dishId}`)
        if (res.ok) reviews.value = await res.json()
    } catch {
        /* 忽略 */
    } finally {
        loading.value = false
    }
}

const submit = async () => {
    const text = content.value.trim()
    if (!text) return
    try {
        const res = await apiFetch(`/reviews/${props.dishId}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ content: text }),
        })
        if (!res.ok) throw new Error()
        const newReview = await res.json()
        reviews.value = [newReview, ...reviews.value]
        content.value = ''
        show('💬 留言成功！', 'success')
    } catch {
        show('留言失敗，請稍後再試', 'error')
    }
}

watch(() => props.dishId, load, { immediate: true })
</script>

<style scoped>
.modal-section-label {
    font-size: 0.7rem;
    letter-spacing: 0.12em;
    text-transform: uppercase;
    color: rgba(249, 221, 211, 0.4);
    margin-bottom: 0.6rem;
    font-family: var(--font-label);
}

.review-loading {
    display: flex;
    align-items: center;
    gap: 0.6rem;
    font-size: 0.8rem;
    color: rgba(249, 221, 211, 0.45);
    padding: 0.5rem 0;
}

.review-list {
    display: flex;
    flex-direction: column;
    gap: 0.6rem;
    margin-bottom: 0.25rem;
}
.review-item {
    background: rgba(255, 255, 255, 0.03);
    border: 1px solid rgba(227, 199, 107, 0.1);
    border-radius: 10px;
    padding: 0.75rem 1rem;
}
.review-meta {
    display: flex;
    justify-content: space-between;
    align-items: baseline;
    margin-bottom: 0.35rem;
    gap: 0.5rem;
}
.review-nickname {
    font-family: var(--font-label);
    font-size: 0.78rem;
    color: var(--eat-primary);
    letter-spacing: 0.04em;
}
.review-time {
    font-family: var(--font-label);
    font-size: 0.65rem;
    color: rgba(249, 221, 211, 0.3);
    white-space: nowrap;
    flex-shrink: 0;
}
.review-content {
    font-family: var(--font-body);
    font-size: 0.83rem;
    line-height: 1.65;
    color: rgba(249, 221, 211, 0.7);
    margin: 0;
    word-break: break-word;
}
.review-more-btn {
    background: none;
    border: 1px solid rgba(227, 199, 107, 0.2);
    border-radius: 20px;
    color: rgba(249, 221, 211, 0.4);
    font-size: 0.72rem;
    padding: 0.3rem 0.85rem;
    cursor: pointer;
    transition:
        border-color 0.2s,
        color 0.2s;
    align-self: flex-start;
}
.review-more-btn:hover {
    border-color: rgba(227, 199, 107, 0.5);
    color: var(--eat-primary);
}
.review-empty {
    font-family: var(--font-body);
    font-size: 0.8rem;
    font-style: italic;
    color: rgba(249, 221, 211, 0.3);
    margin: 0.25rem 0;
}

.rating-divider {
    height: 1px;
    background: rgba(227, 199, 107, 0.1);
    margin: 0.6rem 0 0.9rem;
}

.review-form {
    display: flex;
    flex-direction: column;
    gap: 0.6rem;
}
.review-textarea {
    width: 100%;
    background: rgba(255, 255, 255, 0.04);
    border: 1px solid rgba(227, 199, 107, 0.15);
    border-radius: 10px;
    color: rgba(249, 221, 211, 0.85);
    font-family: var(--font-body);
    font-size: 0.83rem;
    padding: 0.65rem 0.85rem;
    outline: none;
    resize: none;
    box-sizing: border-box;
}
.review-textarea::placeholder {
    color: rgba(249, 221, 211, 0.25);
}
.review-textarea:focus {
    border-color: rgba(227, 199, 107, 0.4);
}

.review-form-footer {
    display: flex;
    justify-content: space-between;
    align-items: center;
    gap: 0.75rem;
}
.review-char-count {
    font-family: var(--font-label);
    font-size: 0.65rem;
    color: rgba(249, 221, 211, 0.3);
    letter-spacing: 0.05em;
    flex-shrink: 0;
}
.review-submit-btn {
    background: rgba(227, 199, 107, 0.1);
    border: 1px solid rgba(227, 199, 107, 0.35);
    border-radius: 20px;
    color: rgba(227, 199, 107, 0.75);
    font-size: 0.75rem;
    padding: 0.35rem 1rem;
    cursor: pointer;
    transition:
        background 0.2s,
        border-color 0.2s;
    white-space: nowrap;
}
.review-submit-btn:hover:not(:disabled) {
    background: rgba(227, 199, 107, 0.2);
    border-color: var(--eat-primary);
}
.review-submit-btn:disabled {
    opacity: 0.35;
    cursor: not-allowed;
}

.login-hint {
    display: flex;
    align-items: center;
    gap: 0.75rem;
    padding: 0.5rem 0;
}
.login-hint-text {
    font-family: var(--font-body);
    font-size: 0.8rem;
    font-style: italic;
    color: rgba(249, 221, 211, 0.35);
    margin: 0;
}
.login-hint-btn {
    background: none;
    border: 1px solid rgba(227, 199, 107, 0.28);
    border-radius: 20px;
    color: rgba(227, 199, 107, 0.65);
    font-size: 0.72rem;
    padding: 0.3rem 0.85rem;
    cursor: pointer;
    transition:
        background 0.2s,
        border-color 0.2s,
        color 0.2s;
    white-space: nowrap;
    flex-shrink: 0;
}
.login-hint-btn:hover {
    background: rgba(227, 199, 107, 0.12);
    border-color: var(--eat-primary);
    color: var(--eat-primary);
}
</style>
