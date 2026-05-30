<template>
    <div v-if="dish" @click.self="$emit('close')" class="modal-wrap open">
        <div class="modal-box">
            <!-- 叉叉關閉按鈕 -->
            <button class="close-btn" @click="$emit('close')" aria-label="關閉">✕</button>
            <div class="detail-img-wrap">
                <img
                    v-if="imgSrc && !imgBroken"
                    :src="imgSrc"
                    :alt="dish.productName"
                    @error="onImgError"
                />
                <div v-else class="detail-img-placeholder">🍽️</div>
            </div>
            <div class="detail-body">
                <!-- 第一排：餐點名稱（左）+ 標籤（右） -->
                <div class="detail-row-1">
                    <h2 class="font-headline detail-name italic" style="color: #e3c76b">
                        {{ dish.productName }}
                    </h2>
                    <div class="detail-tags">
                        <span v-for="tag in tags" :key="tag.key" :class="['badge', tag.cls]">{{
                            tag.label
                        }}</span>
                    </div>
                </div>
                <!-- 第二排：描述 -->
                <p
                    class="font-body detail-desc italic leading-relaxed"
                    style="color: rgba(249, 221, 211, 0.7)"
                >
                    {{ dish.description }}
                </p>
                <!-- 第三排：價格（左）+ 數量控制（右） -->
                <div class="detail-row-3">
                    <span class="font-label detail-price" style="color: #d5b478"
                        >NT$ {{ dish.unitPrice?.toLocaleString() }}</span
                    >
                    <div class="detail-qty">
                        <button class="qty-btn" @click="localQty = Math.max(1, localQty - 1)">
                            −
                        </button>
                        <span class="font-label detail-qty-num" style="color: #f9ddd3">{{
                            localQty
                        }}</span>
                        <button class="qty-btn" @click="localQty++">+</button>
                    </div>
                </div>
                <!-- 備註欄 -->
                <div class="detail-note-wrap">
                    <textarea
                        v-model="note"
                        class="font-body detail-note resize-none"
                        rows="1"
                        placeholder="備註 : 過敏食材、特殊需求…"
                    ></textarea>
                </div>
                <!-- 底部：加入 / 確認修改 按鈕 -->
                <button @click="onConfirm" class="ddm-confirm-btn font-label uppercase">
                    {{ editMode ? '確認修改' : '加入本桌訂單' }}
                </button>
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'

const props = defineProps({
    dish: { type: Object, default: null },
    initialQty: { type: Number, default: 1 },
    initialNote: { type: String, default: '' },
    editMode: { type: Boolean, default: false },
})
const emit = defineEmits(['close', 'confirm'])

const localQty = ref(props.initialQty)
const note = ref(props.initialNote)
const imgSrc = ref('')
const imgFailed = ref(false)
const imgBroken = ref(false)

// 每次 dish / initialQty / initialNote 任一改變時同步（含編輯模式帶入備註）
watch(
    [() => props.dish, () => props.initialQty, () => props.initialNote],
    ([dish, qty, initNote]) => {
        localQty.value = qty || 1
        note.value = initNote || ''
        imgSrc.value = resolveImage(dish?.imageUrl)
        imgFailed.value = false
        imgBroken.value = false
    },
    { immediate: true }
)

const tags = computed(() => {
    if (!props.dish) return []
    const t = []
    if (props.dish.isRecommended) t.push({ key: 'rec', cls: 'badge-new', label: '推薦' })
    if (props.dish.isPopular) t.push({ key: 'pop', cls: 'badge-chef', label: '主廚特選' })
    if (props.dish.isVegetarian) t.push({ key: 'veg', cls: 'badge-veg', label: '素食' })
    if (props.dish.spicyLevel > 0) t.push({ key: 'spicy', cls: 'badge-spicy', label: '辣味' })
    return t
})

function resolveImage(url) {
    if (!url) return ''
    let path = url.replace(/\\/g, '/').replace(/^~\//, '')
    const match = /\/wwwroot\/(.*)/i.exec('/' + path)
    if (match?.[1]) path = match[1]
    return `/${path.replace(/^\//, '')}`
}

function onImgError() {
    if (!imgFailed.value && imgSrc.value.match(/\.jpg$/i)) {
        imgFailed.value = true
        imgSrc.value = imgSrc.value.replace(/\.jpg$/i, '.png')
    } else {
        imgBroken.value = true
    }
}

function onConfirm() {
    emit('confirm', props.dish, localQty.value, note.value)
}
</script>

<style scoped>
.modal-wrap {
    display: none;
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: rgba(24, 11, 6, 0.87);
    backdrop-filter: blur(6px);
    z-index: 9000; /* 高於 notify-toast-stack(8500)，低於 cart-overlay(9500) */
    align-items: center;
    justify-content: center;
}
.modal-wrap.open {
    display: flex;
}
.modal-box {
    background: #2b1c16;
    width: min(580px, 92vw);
    max-height: 90vh;
    overflow-y: auto;
    border-radius: 0.75rem;
    box-shadow: 0 24px 60px rgba(0, 0, 0, 0.6);
    position: relative;
}

.detail-img-wrap {
    position: relative;
    height: 350px;
    overflow: hidden;
}
.detail-img-wrap img {
    width: 100%;
    height: 100%;
    object-fit: cover;
}
.detail-img-placeholder {
    width: 100%;
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    background: #2b1c16;
    color: rgba(208, 197, 181, 0.15);
    font-size: 4rem;
}
.detail-img-wrap::after {
    content: '';
    position: absolute;
    inset: 0;
    background: linear-gradient(to top, #2b1c16 0%, transparent 55%);
}
.close-btn {
    position: absolute;
    top: 0.65rem;
    right: 0.65rem;
    z-index: 10;
    width: 2rem;
    height: 2rem;
    border-radius: 50%;
    background: rgba(24, 11, 6, 0.75);
    border: 1px solid rgba(77, 70, 58, 0.5);
    color: rgba(208, 197, 181, 0.85);
    font-size: 0.9rem;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    transition:
        background 0.2s,
        color 0.2s;
    backdrop-filter: blur(4px);
}
.close-btn:hover {
    background: rgba(43, 28, 22, 0.95);
    color: #e3c76b;
}
.qty-btn {
    width: 28px;
    height: 28px;
    border: 1px solid rgba(77, 70, 58, 0.7);
    background: #2b1c16;
    color: #f9ddd3;
    border-radius: 0.125rem;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 1rem;
    line-height: 1;
    transition:
        border-color 0.3s,
        color 0.3s;
}
.qty-btn:hover {
    border-color: #e3c76b;
    color: #e3c76b;
}
/* ── 內容區整體 ── */
.detail-body {
    display: flex;
    flex-direction: column;
    gap: 0;
    padding: 1.25rem 2rem 0;
}

/* 第一排：名稱＋標籤同行 */
.detail-row-1 {
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: 0.75rem;
    margin-bottom: 0.6rem;
}
.detail-name {
    font-size: 1.6rem;
    line-height: 1.2;
    margin: 0;
    flex: 1;
    min-width: 0;
}
.detail-tags {
    display: flex;
    flex-wrap: wrap;
    gap: 0.3rem;
    flex-shrink: 0;
    align-items: flex-start;
    padding-top: 0.2rem;
}

/* 第二排：描述 */
.detail-desc {
    font-size: 1rem;
    margin: 0 0 1.1rem;
}

/* 第三排：價格＋數量並排 */
.detail-row-3 {
    display: flex;
    align-items: center;
    justify-content: space-between;
    border-top: 1px solid rgba(77, 70, 58, 0.3);
    border-bottom: 1px solid rgba(77, 70, 58, 0.3);
    padding: 0.75rem 0;
    margin-bottom: 1rem;
}
.detail-price {
    font-size: 1.2rem;
    letter-spacing: 0.08em;
}
.detail-qty {
    display: flex;
    align-items: center;
    gap: 0.6rem;
}
.detail-qty-num {
    font-size: 1.1rem;
    width: 2rem;
    text-align: center;
}

/* 備註欄 */
.detail-note-wrap {
    margin-bottom: 0.85rem;
}
.detail-note-label {
    display: block;
    font-size: 0.7rem;
    letter-spacing: 0.18em;
    text-transform: uppercase;
    color: rgba(208, 197, 181, 0.45);
    margin-bottom: 0.35rem;
}
.detail-note {
    width: 100%;
    background: rgba(24, 11, 6, 0.5);
    border: 1px solid rgba(77, 70, 58, 0.55);
    border-radius: 0.25rem;
    color: #f9ddd3;
    font-size: 0.95rem;
    padding: 0.55rem 0.75rem;
    outline: none;
    transition: border-color 0.25s;
    box-sizing: border-box;
}
.detail-note:focus {
    border-color: rgba(227, 199, 107, 0.5);
}
.detail-note::placeholder {
    color: rgba(208, 197, 181, 0.28);
}

/* 底部確認按鈕（獨立命名，不與父元件 .submit-btn 衝突） */
.ddm-confirm-btn {
    display: block;
    width: 100%;
    padding: 0.9rem 0;
    margin-bottom: 1.5rem;
    border: none;
    border-radius: 0;
    cursor: pointer;
    font-size: 0.85rem;
    letter-spacing: 0.28em;
    background: linear-gradient(to right, #e3c76b, #c6ab53);
    color: #3b2f00;
    transition: filter 0.2s, transform 0.15s;
}
.ddm-confirm-btn:hover {
    filter: brightness(1.1);
}
.ddm-confirm-btn:active {
    transform: scale(0.97);
}

.badge-new {
    background: rgba(227, 199, 107, 0.18);
    border: 1px solid rgba(227, 199, 107, 0.45);
    color: #e3c76b;
}
.badge-veg {
    background: rgba(66, 49, 43, 0.5);
    border: 1px solid rgba(120, 180, 80, 0.4);
    color: #a3d977;
}
.badge-spicy {
    background: rgba(147, 0, 10, 0.25);
    border: 1px solid rgba(255, 100, 80, 0.4);
    color: #ffb4ab;
}
.badge-chef {
    background: rgba(93, 69, 20, 0.4);
    border: 1px solid rgba(228, 194, 133, 0.35);
    color: #e4c285;
}
.badge {
    font-family: 'Work Sans', sans-serif;
    font-size: 0.58rem;
    letter-spacing: 0.18em;
    text-transform: uppercase;
    padding: 0.18rem 0.5rem;
}
.font-headline {
    font-family: 'Noto Serif TC', serif;
}
.font-label {
    font-family: 'Work Sans', sans-serif;
}
.font-body {
    font-family: 'Newsreader', serif;
}

/* ════════════════════════════════════════════
   手機版專用  (max-width: 600px)
   完全獨立，不繼承上方任何樣式
   ════════════════════════════════════════════ */
@media (max-width: 600px) {
    .modal-box {
        background: #2b1c16;
        width: min(420px, 92vw);
        max-height: 80vh;
        overflow-y: auto;
        border-radius: 0.75rem;
        box-shadow: 0 24px 60px rgba(0, 0, 0, 0.6);
        position: relative;
    }

    .detail-img-wrap {
        position: relative;
        height: 250px;
        overflow: hidden;
        flex-shrink: 0;
    }
    .detail-img-wrap img {
        width: 100%;
        height: 100%;
        object-fit: cover;
    }
    .detail-img-wrap::after {
        content: '';
        position: absolute;
        inset: 0;
        background: linear-gradient(to top, #2b1c16 0%, transparent 55%);
    }

    .detail-body {
        display: flex;
        flex-direction: column;
        gap: 0;
        padding: 0.75rem 1.5rem 0.9rem;
    }

    .detail-row-1 {
        display: flex;
        align-items: flex-start;
        justify-content: space-between;
        gap: 0.75rem;
        margin-bottom: 0.35rem;
    }
    .detail-name {
        font-size: 1.3rem;
        line-height: 1.2;
        margin: 0;
        flex: 1;
        min-width: 0;
    }

    .detail-desc {
        font-size: 0.8rem;
        margin: 0 0 0.3rem;
        display: -webkit-box;
        -webkit-line-clamp: 2;
        -webkit-box-orient: vertical;
        overflow: hidden;
    }

    .detail-row-3 {
        display: flex;
        align-items: center;
        justify-content: space-between;
        border-top: 1px solid rgba(77, 70, 58, 0.3);
        border-bottom: 1px solid rgba(77, 70, 58, 0.3);
        padding: 0.35rem 0;
        margin-bottom: 0.4rem;
    }

    .detail-note-wrap {
        margin-bottom: 0.35rem;
    }
    .detail-note {
        width: 100%;
        background: rgba(24, 11, 6, 0.5);
        border: 1px solid rgba(77, 70, 58, 0.55);
        border-radius: 0.25rem;
        color: #f9ddd3;
        font-size: 0.8rem;
        padding: 0.4rem 0.65rem;
        outline: none;
        transition: border-color 0.25s;
        box-sizing: border-box;
    }
    .ddm-confirm-btn {
        padding: 0.65rem 0;
        margin-bottom: 0.6rem;
    }
}
</style>
