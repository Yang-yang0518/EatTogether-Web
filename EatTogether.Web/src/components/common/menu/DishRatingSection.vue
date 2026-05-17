<!-- 星星評分（含 localStorage 防重複） -->
<template>
    <div class="rating-section">
        <template v-if="isLoggedIn">
            <div class="modal-section-label">為這道餐點評分</div>
            <div class="star-row">
                <button
                    v-for="star in 5"
                    :key="star"
                    class="star-btn"
                    :class="{ 'is-rated': alreadyRated }"
                    :disabled="alreadyRated"
                    @click="submitRating(star)"
                    @mouseenter="!alreadyRated && (hoverStar = star)"
                    @mouseleave="hoverStar = 0"
                    :aria-label="`${star} 顆星`"
                >
                    {{ (hoverStar || localStar || 0) >= star ? '★' : '☆' }}
                </button>
            </div>
            <p v-if="alreadyRated" class="star-voted">已評 {{ ratedScore }} 顆星 ★</p>
        </template>
        <div v-else class="login-hint">
            <span class="login-hint-text">登入後即可為餐點評分</span>
            <button class="login-hint-btn" @click="emit('login')">立即登入</button>
        </div>
    </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import apiFetch from '@/utils/apiFetch.js'
import { useToast } from '@/composables/useToast.js'
import { useAuthStore } from '@/stores/auth.js'

const props = defineProps({
    dish: { type: Object, required: true },
    isLoggedIn: { type: Boolean, default: false },
})

const emit = defineEmits(['rated', 'login'])

const { show } = useToast()
const authStore = useAuthStore()

const hoverStar = ref(0)
const localStar = ref(0)

const _ratingKey = () => (authStore.member?.id ? `ratings-${authStore.member.id}` : null)

const ratedMap = computed(() => {
    const key = _ratingKey()
    return key ? JSON.parse(localStorage.getItem(key) || '{}') : {}
})

const alreadyRated = computed(() => props.isLoggedIn && !!ratedMap.value[props.dish.id])
const ratedScore   = computed(() => ratedMap.value[props.dish.id] || 0)

const submitRating = async (star) => {
    if (alreadyRated.value) return
    try {
        const res = await apiFetch(`/Dishes/${props.dish.id}/Rate`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ score: star }),
        })
        if (!res.ok) throw new Error()
        const data = await res.json()
        const key = _ratingKey()
        if (key) {
            const stored = { ...ratedMap.value }
            stored[props.dish.id] = star
            localStorage.setItem(key, JSON.stringify(stored))
        }
        localStar.value = star
        show('⭐ 感謝您的評分！', 'success')
        emit('rated', { dishId: props.dish.id, star, ...data })
    } catch {
        show('評分失敗，請稍後再試', 'error')
    }
}
</script>

<style scoped>
.rating-section {
    /* 由父層 .modal-section 負責間距 */
}

.modal-section-label {
    font-size: 0.7rem;
    letter-spacing: 0.12em;
    text-transform: uppercase;
    color: rgba(249, 221, 211, 0.4);
    margin-bottom: 0.6rem;
    font-family: var(--font-label);
}

.star-row {
    display: flex;
    gap: 0.25rem;
    margin-bottom: 0.5rem;
}
.star-btn {
    font-size: 1.5rem;
    cursor: pointer;
    background: none;
    border: none;
    padding: 0 0.1rem;
    line-height: 1;
    color: var(--eat-primary);
    transition: transform 0.15s;
}
.star-btn:hover {
    transform: scale(1.2);
}
.star-btn.is-rated {
    cursor: default;
    opacity: 0.75;
}
.star-btn:disabled {
    pointer-events: none;
}

.star-voted {
    font-family: var(--font-label);
    font-size: 0.7rem;
    color: rgba(249, 221, 211, 0.45);
    margin: 0;
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
