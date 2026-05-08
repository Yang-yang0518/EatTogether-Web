<template>
    <div v-if="meal" class="sm-wrap" @click.self="$emit('close')">
        <div class="sm-box">
            <!-- 圖片 -->
            <div class="sm-img-wrap">
                <img v-if="meal.imageUrl" :src="meal.imageUrl" :alt="meal.setMealName" />
                <div v-else class="sm-img-ph">
                    <span>{{ meal.setMealName?.charAt(0) }}</span>
                </div>
                <button class="sm-close font-label" @click="$emit('close')">✕</button>
                <div class="sm-img-fade"></div>
            </div>

            <!-- 主體 -->
            <div class="sm-body">
                <!-- 標題 + 價格 -->
                <div class="sm-head">
                    <h2 class="font-headline sm-name">{{ meal.setMealName }}</h2>
                    <div class="sm-price-row">
                        <span class="font-label sm-price"
                            >NT$ {{ meal.setPrice?.toLocaleString() }}</span
                        >
                        <div v-if="editMode" class="sm-qty-ctrl">
                            <button
                                class="sm-qty-btn font-label"
                                @click="qty = Math.max(1, qty - 1)"
                            >
                                −
                            </button>
                            <span class="font-label sm-qty-num">{{ qty }}</span>
                            <button class="sm-qty-btn font-label" @click="qty++">+</button>
                        </div>
                        <div v-else class="sm-qty-ctrl">
                            <button
                                class="sm-qty-btn font-label"
                                @click="qty = Math.max(1, qty - 1)"
                            >
                                −
                            </button>
                            <span class="font-label sm-qty-num">{{ qty }}</span>
                            <button class="sm-qty-btn font-label" @click="qty++">+</button>
                        </div>
                    </div>
                    <p v-if="meal.description" class="font-body sm-desc">{{ meal.description }}</p>
                </div>

                <!-- 固定餐點 -->
                <div v-if="fixedItems.length" class="sm-section">
                    <p class="sm-section-label font-label">固定餐點</p>
                    <div v-for="item in fixedItems" :key="'f-' + item.dishId" class="sm-fixed-row">
                        <span class="font-body sm-item-name">{{ item.dishName }}</span>
                        <span class="font-label sm-item-qty">× {{ item.quantity }}</span>
                    </div>
                </div>

                <!-- 選填組 -->
                <div v-for="group in optionalGroups" :key="'g-' + group.groupNo" class="sm-group">
                    <div class="sm-group-head">
                        <span class="font-label sm-group-title"
                            >請選擇 {{ group.pickLimit }} 項</span
                        >
                        <span
                            :class="[
                                'font-label sm-group-status',
                                groupIsFull(group) ? 'status-done' : 'status-pending',
                            ]"
                        >
                            已選 {{ groupSelected(group) }} / {{ group.pickLimit }}
                        </span>
                    </div>
                    <p v-if="!groupIsFull(group)" class="font-label sm-group-hint">
                        尚未選擇（共需選 {{ group.pickLimit }} 項）
                    </p>

                    <div v-for="opt in group.options" :key="opt.dishId" class="sm-opt-row">
                        <span class="font-body sm-opt-name">{{ opt.dishName }}</span>
                        <div class="sm-opt-qty">
                            <button
                                class="sm-qty-btn sm-qty-btn-sm font-label"
                                :disabled="!getOptQty(group.groupNo, opt.dishId)"
                                @click="decOpt(group.groupNo, opt)"
                            >
                                −
                            </button>
                            <span class="font-label sm-qty-num sm-qty-num-sm">
                                {{ getOptQty(group.groupNo, opt.dishId) }}
                            </span>
                            <button
                                class="sm-qty-btn sm-qty-btn-sm font-label"
                                :disabled="
                                    groupIsFull(group) && !getOptQty(group.groupNo, opt.dishId)
                                "
                                @click="incOpt(group.groupNo, opt)"
                            >
                                +
                            </button>
                        </div>
                    </div>
                </div>

                <!-- 備註 -->
                <div class="sm-note-wrap">
                    <textarea
                        v-model="note"
                        class="font-body sm-note resize-none"
                        rows="2"
                        placeholder="備註：過敏食材、特殊需求…"
                    ></textarea>
                </div>

                <!-- 確認按鈕 -->
                <button
                    @click="onConfirm"
                    :disabled="!isValid"
                    :class="['sm-submit font-label', { 'sm-submit-disabled': !isValid }]"
                >
                    <template v-if="isValid">
                        {{ editMode ? '確認修改' : '加入本桌訂單' }}
                    </template>
                    <template v-else> 尚有 {{ pendingGroups }} 組未完成選擇 </template>
                </button>
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref, computed, watch, reactive } from 'vue'

const props = defineProps({
    meal: { type: Object, default: null },
    editMode: { type: Boolean, default: false },
    initialQty: { type: Number, default: 1 },
    // { groupNo: { dishId: qty } } 用於編輯時帶入舊選項
    initialSel: { type: Object, default: () => ({}) },
    initialNote: { type: String, default: '' },
})
const emit = defineEmits(['close', 'confirm'])

const qty = ref(1)
const note = ref('')
// { [groupNo]: { [dishId]: qty } }
const selections = reactive({})

// ── computed ──────────────────────────────────────────────────────
const fixedItems = computed(() => (props.meal?.items ?? []).filter((i) => !i.isOptional))

const optionalGroups = computed(() => {
    const map = {}
    for (const item of props.meal?.items ?? []) {
        if (!item.isOptional) continue
        const gno = item.optionGroupNo
        if (!map[gno]) map[gno] = { groupNo: gno, pickLimit: item.pickLimit, options: [] }
        map[gno].options.push(item)
    }
    return Object.values(map).sort((a, b) => a.groupNo - b.groupNo)
})

function getOptQty(groupNo, dishId) {
    return selections[groupNo]?.[dishId] ?? 0
}

function groupSelected(group) {
    return Object.values(selections[group.groupNo] ?? {}).reduce((s, v) => s + v, 0)
}

function groupIsFull(group) {
    return groupSelected(group) >= group.pickLimit
}

const isValid = computed(() => optionalGroups.value.every((g) => groupSelected(g) === g.pickLimit))

const pendingGroups = computed(
    () => optionalGroups.value.filter((g) => groupSelected(g) < g.pickLimit).length
)

// ── actions ───────────────────────────────────────────────────────
function incOpt(groupNo, opt) {
    const group = optionalGroups.value.find((g) => g.groupNo === groupNo)
    if (groupIsFull(group) && !getOptQty(groupNo, opt.dishId)) return
    if (!selections[groupNo]) selections[groupNo] = {}
    if (groupIsFull(group)) return // 已達上限
    selections[groupNo][opt.dishId] = getOptQty(groupNo, opt.dishId) + 1
}

function decOpt(groupNo, opt) {
    const cur = getOptQty(groupNo, opt.dishId)
    if (!cur) return
    if (!selections[groupNo]) return
    if (cur - 1 <= 0) delete selections[groupNo][opt.dishId]
    else selections[groupNo][opt.dishId] = cur - 1
}

function onConfirm() {
    if (!isValid.value) return
    const selectedOptions = []
    for (const group of optionalGroups.value) {
        for (const [dishId, q] of Object.entries(selections[group.groupNo] ?? {})) {
            if (q <= 0) continue
            const opt = group.options.find((o) => o.dishId === Number(dishId))
            if (opt)
                selectedOptions.push({
                    dishId: opt.dishId,
                    dishName: opt.dishName,
                    qty: q,
                    groupNo: group.groupNo,
                })
        }
    }
    emit('confirm', props.meal, qty.value, note.value, selectedOptions)
}

// ── 初始化 ────────────────────────────────────────────────────────
function applyInitial() {
    qty.value = props.initialQty || 1
    note.value = props.initialNote || ''
    Object.keys(selections).forEach((k) => delete selections[k])
    if (props.initialSel) {
        for (const [gno, dishMap] of Object.entries(props.initialSel)) {
            selections[gno] = { ...dishMap }
        }
    }
}

watch(
    [() => props.meal, () => props.initialSel, () => props.initialNote, () => props.initialQty],
    applyInitial
)
</script>

<style scoped>
.sm-wrap {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: rgba(24, 11, 6, 0.87);
    backdrop-filter: blur(6px);
    z-index: 9000; /* 高於 notify-toast-stack(8500)，低於 cart-overlay(9500) */
    display: flex;
    align-items: center;
    justify-content: center;
}
.sm-box {
    background: #2b1c16;
    width: min(560px, 95vw);
    max-height: 90vh;
    overflow-y: auto;
    box-shadow: 0 24px 60px rgba(0, 0, 0, 0.6);
    border-radius: 0.125rem;
}

/* 圖片 */
.sm-img-wrap {
    position: relative;
    height: 200px;
    overflow: hidden;
    flex-shrink: 0;
}
.sm-img-wrap img {
    width: 100%;
    height: 100%;
    object-fit: cover;
}
.sm-img-ph {
    width: 100%;
    height: 100%;
    background: #3a2518;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 4rem;
    color: rgba(227, 199, 107, 0.3);
    font-family: 'Noto Serif TC', serif;
}
.sm-img-fade {
    position: absolute;
    inset: 0;
    background: linear-gradient(to top, #2b1c16 0%, transparent 55%);
}
.sm-close {
    position: absolute;
    top: 0.75rem;
    right: 0.75rem;
    z-index: 2;
    width: 2rem;
    height: 2rem;
    border-radius: 50%;
    background: rgba(24, 11, 6, 0.7);
    border: 1px solid rgba(77, 70, 58, 0.4);
    color: rgba(208, 197, 181, 0.8);
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 0.9rem;
    transition: color 0.2s;
}
.sm-close:hover {
    color: #e3c76b;
}

/* 主體 */
.sm-body {
    padding: 1.25rem 1.75rem 0;
    display: flex;
    flex-direction: column;
    gap: 0;
}
.sm-head {
    margin-bottom: 1rem;
}
.sm-name {
    font-size: 1.5rem;
    color: #e3c76b;
    margin: 0 0 0.4rem;
}
.sm-price-row {
    display: flex;
    align-items: center;
    gap: 1.25rem;
    margin-bottom: 0.5rem;
}
.sm-price {
    font-size: 1.15rem;
    color: #d5b478;
    letter-spacing: 0.06em;
}
.sm-desc {
    font-size: 0.9rem;
    color: rgba(249, 221, 211, 0.65);
    margin: 0;
    line-height: 1.5;
}

/* 固定餐點 */
.sm-section {
    border-top: 1px solid rgba(77, 70, 58, 0.3);
    padding: 0.75rem 0 0.5rem;
    margin-bottom: 0.25rem;
}
.sm-section-label {
    font-size: 0.68rem;
    letter-spacing: 0.2em;
    text-transform: uppercase;
    color: rgba(208, 197, 181, 0.45);
    margin-bottom: 0.45rem;
}
.sm-fixed-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 0.2rem 0;
}
.sm-item-name {
    font-size: 0.9rem;
    color: rgba(249, 221, 211, 0.75);
}
.sm-item-qty {
    font-size: 0.8rem;
    color: rgba(208, 197, 181, 0.5);
}

/* 選填組 */
.sm-group {
    border-top: 1px solid rgba(77, 70, 58, 0.3);
    padding: 0.75rem 0 0.5rem;
}
.sm-group-head {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 0.25rem;
}
.sm-group-title {
    font-size: 0.85rem;
    color: rgba(249, 221, 211, 0.9);
    letter-spacing: 0.06em;
}
.sm-group-status {
    font-size: 0.75rem;
    letter-spacing: 0.08em;
}
.status-done {
    color: #a3d977;
}
.status-pending {
    color: rgba(208, 197, 181, 0.45);
}
.sm-group-hint {
    font-size: 0.72rem;
    color: rgba(208, 197, 181, 0.4);
    letter-spacing: 0.08em;
    margin-bottom: 0.4rem;
}

/* 選項列 */
.sm-opt-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 0.35rem 0;
    border-bottom: 1px solid rgba(77, 70, 58, 0.12);
}
.sm-opt-row:last-child {
    border-bottom: none;
}
.sm-opt-name {
    font-size: 0.9rem;
    color: rgba(249, 221, 211, 0.85);
}
.sm-opt-qty {
    display: flex;
    align-items: center;
    gap: 0.5rem;
}

/* 數量控制 */
.sm-qty-ctrl {
    display: flex;
    align-items: center;
    gap: 0.5rem;
}
.sm-qty-btn {
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
    transition:
        border-color 0.2s,
        color 0.2s;
}
.sm-qty-btn:hover:not(:disabled) {
    border-color: #e3c76b;
    color: #e3c76b;
}
.sm-qty-btn:disabled {
    opacity: 0.3;
    cursor: default;
}
.sm-qty-btn-sm {
    width: 24px;
    height: 24px;
    font-size: 0.9rem;
}
.sm-qty-num {
    font-size: 1rem;
    color: #f9ddd3;
    width: 1.8rem;
    text-align: center;
}
.sm-qty-num-sm {
    font-size: 0.88rem;
    width: 1.4rem;
}

/* 備註 */
.sm-note-wrap {
    border-top: 1px solid rgba(77, 70, 58, 0.3);
    padding-top: 0.75rem;
    margin-top: 0.5rem;
    margin-bottom: 0.85rem;
}
.sm-note {
    width: 100%;
    background: rgba(24, 11, 6, 0.5);
    border: 1px solid rgba(77, 70, 58, 0.55);
    border-radius: 0.25rem;
    color: #f9ddd3;
    font-size: 0.95rem;
    padding: 0.55rem 0.75rem;
    outline: none;
    box-sizing: border-box;
    transition: border-color 0.25s;
}
.sm-note:focus {
    border-color: rgba(227, 199, 107, 0.5);
}
.sm-note::placeholder {
    color: rgba(208, 197, 181, 0.28);
}

/* 送出 */
.sm-submit {
    width: 100%;
    display: block;
    padding: 0.9rem 0;
    font-size: 0.85rem;
    letter-spacing: 0.28em;
    text-transform: uppercase;
    margin-bottom: 1.5rem;
    border: none;
    cursor: pointer;
    background: linear-gradient(to right, #e3c76b, #c6ab53);
    color: #3b2f00;
    transition:
        filter 0.2s,
        transform 0.15s;
    border-radius: 0.125rem;
}
.sm-submit:hover:not(.sm-submit-disabled) {
    filter: brightness(1.1);
}
.sm-submit:active:not(.sm-submit-disabled) {
    transform: scale(0.97);
}
.sm-submit-disabled {
    background: rgba(77, 70, 58, 0.4);
    color: rgba(208, 197, 181, 0.4);
    cursor: not-allowed;
    letter-spacing: 0.1em;
}
</style>
