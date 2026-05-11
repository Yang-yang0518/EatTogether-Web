<template>
    <div class="ol-page">
        <div class="ol-container">
            <div class="ol-cards-wrap">
                <!-- ══ 左卡：搜尋面板 ══ -->
                <div class="ol-card ol-search-card">
                    <div class="ol-card-title font-headline">搜尋</div>
                    <div class="ol-card-divider"></div>

                    <div class="ol-field">
                        <label class="ol-label font-label">
                            取餐人姓名
                            <span class="ol-required">（必填）</span>
                        </label>
                        <input
                            v-model="lkName"
                            class="ol-input font-label"
                            placeholder="請輸入完整姓名"
                            @keyup.enter="doLookup"
                        />
                    </div>
                    <div class="ol-card-divider" style="margin: 0.5rem 0"></div>
                    <div class="ol-field">
                        <label class="ol-label font-label">
                            訂單編號
                            <span class="ol-required">（查單筆）</span>
                        </label>
                        <input
                            v-model="lkOrderNum"
                            class="ol-input font-label"
                            placeholder="例：20260503-0001"
                            @input="lkPhone = ''"
                            @keyup.enter="doLookup"
                        />
                    </div>

                    <div class="ol-field">
                        <label class="ol-label font-label">
                            電話號碼
                            <span class="ol-required">（查多筆）</span>
                        </label>
                        <input
                            v-model="lkPhone"
                            class="ol-input font-label"
                            placeholder="請輸入完整電話"
                            @input="lkOrderNum = ''"
                            @keyup.enter="doLookup"
                        />
                    </div>

                    <p v-if="error" class="ol-error font-label">{{ error }}</p>

                    <!-- 展示用：快速填入測試資料 -->
                    <button class="ol-demo-btn font-label" @click="fillDemo">▶ 展示</button>

                    <div class="ol-btn-group">
                        <button class="ol-reset-btn font-label" @click="resetSearch">✖ 重置</button>
                        <button
                            class="ol-search-btn font-label"
                            :disabled="!canSearch || searching"
                            @click="doLookup"
                        >
                            <svg
                                v-if="!searching"
                                xmlns="http://www.w3.org/2000/svg"
                                width="14"
                                height="14"
                                fill="currentColor"
                                viewBox="0 0 16 16"
                            >
                                <path
                                    d="M11.742 10.344a6.5 6.5 0 1 0-1.397 1.398h-.001c.03.04.062.078.098.115l3.85 3.85a1 1 0 0 0 1.415-1.414l-3.85-3.85a1.007 1.007 0 0 0-.115-.1zM12 6.5a5.5 5.5 0 1 1-11 0 5.5 5.5 0 0 1 11 0z"
                                />
                            </svg>
                            <span v-if="searching" class="ol-spinner"></span>
                            {{ searching ? '查詢中…' : '搜尋' }}
                        </button>
                    </div>

                    <!-- 查詢結果訂單編號列表 -->
                    <template v-if="searched && results.length > 0">
                        <div class="ol-card-divider" style="margin-top: 0.5rem"></div>
                        <p class="ol-sidebar-result-label font-label">查詢結果</p>
                        <div class="ol-sidebar-order-list">
                            <button
                                v-for="r in results"
                                :key="r.orderNumber"
                                :class="[
                                    'ol-sidebar-order-item font-label',
                                    {
                                        active:
                                            selectedOrder &&
                                            selectedOrder.orderNumber === r.orderNumber,
                                    },
                                ]"
                                @click="selectedOrder = r"
                            >
                                {{ r.orderNumber }}
                            </button>
                        </div>
                    </template>
                </div>

                <!-- ══ 右卡：訂單詳情 ══ -->
                <div class="ol-card ol-results-card">
                    <div class="ol-card-header">
                        <div class="ol-card-title font-headline">訂單查詢</div>
                        <span
                            v-if="searched && !searching && results.length > 0"
                            class="font-label ol-count"
                        >
                            共 {{ results.length }} 項
                        </span>
                    </div>
                    <div class="ol-card-divider"></div>

                    <!-- 初始提示 -->
                    <div v-if="!searched && !searching" class="ol-empty">
                        <div class="ol-empty-icon">🔍</div>
                        <p class="font-body ol-empty-text">請使用左側面板查詢訂單進度</p>
                    </div>

                    <!-- 搜尋中 -->
                    <div v-else-if="searching" class="ol-empty">
                        <div class="ol-loading-dots"><span></span><span></span><span></span></div>
                        <p
                            class="font-label"
                            style="color: rgba(208, 197, 181, 0.4); margin-top: 1rem"
                        >
                            查詢中…
                        </p>
                    </div>

                    <!-- 查無結果 -->
                    <div v-else-if="searched && results.length === 0" class="ol-empty">
                        <div class="ol-empty-icon">📭</div>
                        <p class="font-body ol-empty-text">查無符合的訂單</p>
                        <p class="font-label ol-empty-hint">
                            請確認資料是否填寫正確，查詢範圍僅限當日外帶訂單
                        </p>
                    </div>

                    <!-- 有結果：直接顯示詳情 -->
                    <template v-else-if="selectedOrder">
                        <!-- 訂單編號列 -->
                        <div class="ol-order-header">
                            <div class="ol-order-num-block">
                                <span class="font-label ol-order-num-label">訂單編號</span>
                                <strong class="ol-order-num-val">{{
                                    selectedOrder.orderNumber
                                }}</strong>
                            </div>
                        </div>

                        <!-- 進度條 -->
                        <!-- status=0：三步驟（接收✓ → 製作中▶ → 完成暗） -->
                        <div v-if="selectedOrder.orderStatus === 0" class="ol-progress">
                            <div class="ol-prog-step ol-prog-done">
                                <div class="ol-prog-dot">
                                    <svg
                                        xmlns="http://www.w3.org/2000/svg"
                                        width="14"
                                        height="14"
                                        fill="currentColor"
                                        viewBox="0 0 16 16"
                                    >
                                        <path
                                            d="M13.854 3.646a.5.5 0 0 1 0 .708l-7 7a.5.5 0 0 1-.708 0l-3.5-3.5a.5.5 0 1 1 .708-.708L6.5 10.293l6.646-6.647a.5.5 0 0 1 .708 0z"
                                        />
                                    </svg>
                                </div>
                                <span class="font-label ol-prog-lbl">訂單已接收</span>
                            </div>
                            <div class="ol-prog-line ol-prog-line-lit"></div>
                            <div class="ol-prog-step ol-prog-active">
                                <div class="ol-prog-dot"><span>2</span></div>
                                <span class="font-label ol-prog-lbl">餐點製作中</span>
                            </div>
                            <div class="ol-prog-line"></div>
                            <div class="ol-prog-step">
                                <div class="ol-prog-dot"><span>3</span></div>
                                <span class="font-label ol-prog-lbl">餐點已完成</span>
                            </div>
                        </div>

                        <!-- status=1：餐點已完成（待取餐）-->
                        <div
                            v-else-if="selectedOrder.orderStatus === 1"
                            class="ol-progress-done-single"
                        >
                            <div class="ol-done-single-badge">
                                <svg
                                    xmlns="http://www.w3.org/2000/svg"
                                    width="18"
                                    height="18"
                                    fill="currentColor"
                                    viewBox="0 0 16 16"
                                >
                                    <path
                                        d="M13.854 3.646a.5.5 0 0 1 0 .708l-7 7a.5.5 0 0 1-.708 0l-3.5-3.5a.5.5 0 1 1 .708-.708L6.5 10.293l6.646-6.647a.5.5 0 0 1 .708 0z"
                                    />
                                </svg>
                                <span class="font-label">餐點已完成</span>
                            </div>
                        </div>

                        <!-- status=3：已結帳完成 -->
                        <div
                            v-else-if="selectedOrder.orderStatus === 3"
                            class="ol-progress-done-single"
                        >
                            <div class="ol-done-single-badge ol-done-paid-badge">
                                <svg
                                    xmlns="http://www.w3.org/2000/svg"
                                    width="18"
                                    height="18"
                                    fill="currentColor"
                                    viewBox="0 0 16 16"
                                >
                                    <path
                                        d="M13.854 3.646a.5.5 0 0 1 0 .708l-7 7a.5.5 0 0 1-.708 0l-3.5-3.5a.5.5 0 1 1 .708-.708L6.5 10.293l6.646-6.647a.5.5 0 0 1 .708 0z"
                                    />
                                </svg>
                                <span class="font-label">已結帳完成</span>
                            </div>
                        </div>

                        <!-- status=2：三步驟全暗（已取消） -->
                        <div
                            v-else-if="selectedOrder.orderStatus === 2"
                            class="ol-progress ol-progress-cancelled"
                        >
                            <div class="ol-prog-step">
                                <div class="ol-prog-dot"><span>1</span></div>
                                <span class="font-label ol-prog-lbl">訂單已接收</span>
                            </div>
                            <div class="ol-prog-line"></div>
                            <div class="ol-prog-step">
                                <div class="ol-prog-dot"><span>2</span></div>
                                <span class="font-label ol-prog-lbl">餐點製作中</span>
                            </div>
                            <div class="ol-prog-line"></div>
                            <div class="ol-prog-step">
                                <div class="ol-prog-dot"><span>3</span></div>
                                <span class="font-label ol-prog-lbl">餐點已完成</span>
                            </div>
                        </div>

                        <!-- 雙欄主體（與訂單已送出頁面相同排版）-->
                        <div class="ol-detail-cols">
                            <!-- 左欄：餐點明細 -->
                            <div class="ol-detail-left">
                                <h3 class="font-label ol-section-title">餐點明細</h3>
                                <div class="ol-items-detail">
                                    <div
                                        v-for="(item, idx) in selectedOrder.items"
                                        :key="idx"
                                        :class="['ol-item', { 'ol-item-gift': item.isGift }]"
                                    >
                                        <div class="ol-item-left">
                                            <div
                                                v-if="item.isSetMeal"
                                                class="setmeal-badge-sm font-label"
                                            >
                                                🍱 套餐
                                            </div>
                                            <span class="font-body ol-item-name">{{
                                                item.productName
                                            }}</span>
                                            <div v-if="item.subItems?.length" class="ol-subitems">
                                                <span
                                                    v-for="s in item.subItems"
                                                    :key="s"
                                                    class="ol-subitem font-label"
                                                    >{{ s }}</span
                                                >
                                            </div>
                                            <span
                                                v-if="item.itemNote"
                                                class="font-label ol-item-note"
                                                >{{ item.itemNote }}</span
                                            >
                                        </div>
                                        <div class="ol-item-right">
                                            <span class="font-label ol-item-qty"
                                                >× {{ item.qty }}</span
                                            >
                                            <span
                                                v-if="item.isGift"
                                                class="gift-order-badge font-label"
                                                >🎁 贈品</span
                                            >
                                            <span v-else class="font-label ol-item-price"
                                                >NT$
                                                {{
                                                    (item.unitPrice * item.qty).toLocaleString()
                                                }}</span
                                            >
                                        </div>
                                    </div>
                                    <!-- 贈品活動來源 -->
                                    <div
                                        v-if="
                                            selectedOrder.eventTitle &&
                                            selectedOrder.eventDiscountType === 'Gift'
                                        "
                                        class="ol-gift-event-note font-label"
                                    >
                                        {{ selectedOrder.eventTitle }}：贈品贈送
                                    </div>
                                </div>

                                <div
                                    class="feather-divider"
                                    style="margin: 0.85rem 0 0.75rem"
                                ></div>

                                <div class="ol-meta-rows">
                                    <!-- 活動（非贈品型） -->
                                    <div
                                        v-if="
                                            selectedOrder.eventTitle &&
                                            selectedOrder.eventDiscountType !== 'Gift'
                                        "
                                        class="ol-meta-row-3"
                                    >
                                        <span class="font-label ol-meta-label">活動</span>
                                        <span class="font-label ol-meta-val-mid">{{
                                            selectedOrder.eventTitle
                                        }}</span>
                                        <span class="font-label ol-meta-discount"
                                            >折抵 NT$
                                            {{ selectedOrder.eventDiscount.toLocaleString() }}</span
                                        >
                                    </div>
                                    <!-- 優惠券 -->
                                    <div v-if="selectedOrder.couponCode" class="ol-meta-row-3">
                                        <span class="font-label ol-meta-label">優惠券</span>
                                        <span class="font-label ol-meta-val-mid">{{
                                            selectedOrder.couponCode
                                        }}</span>
                                        <span class="font-label ol-meta-discount"
                                            >折抵 NT$
                                            {{
                                                selectedOrder.couponDiscount.toLocaleString()
                                            }}</span
                                        >
                                    </div>
                                    <!-- 合計 -->
                                    <div class="ol-meta-row">
                                        <span class="font-label ol-meta-label-dim">合計</span>
                                        <span class="font-label ol-meta-val"
                                            >NT$ {{ selectedOrder.subtotal.toLocaleString() }}</span
                                        >
                                    </div>
                                    <!-- 折扣 -->
                                    <div
                                        v-if="selectedOrder.discountAmount > 0"
                                        class="ol-meta-row"
                                    >
                                        <span class="font-label ol-meta-label-dim">折扣</span>
                                        <span class="font-label" style="color: #7ec87e"
                                            >－ NT$
                                            {{
                                                selectedOrder.discountAmount.toLocaleString()
                                            }}</span
                                        >
                                    </div>
                                </div>

                                <div
                                    class="feather-divider"
                                    style="margin: 0.65rem 0 0.55rem"
                                ></div>
                                <div class="ol-meta-row">
                                    <span
                                        class="font-label ol-meta-label-dim"
                                        style="font-size: 0.95rem"
                                        >金額總計</span
                                    >
                                    <span class="font-label" style="color: #e3c76b; font-size: 1rem"
                                        >NT$ {{ selectedOrder.totalAmount.toLocaleString() }}</span
                                    >
                                </div>
                                <!-- 備註 -->
                                <div
                                    v-if="displayNote(selectedOrder.note)"
                                    class="ol-meta-row"
                                    style="margin-top: 0.45rem"
                                >
                                    <span class="font-label ol-meta-label-dim">備註</span>
                                    <span class="font-label ol-meta-val">{{
                                        displayNote(selectedOrder.note)
                                    }}</span>
                                </div>
                            </div>

                            <!-- 右欄：提醒 + 取餐資訊 + 聯絡 -->
                            <div class="ol-detail-right">
                                <!-- 防呆提醒：進行中 & 已完成訂單 -->
                                <div v-if="selectedOrder.orderStatus !== 2" class="ol-reminders">
                                    <div class="ol-reminder-item">
                                        <span class="ol-reminder-icon">⏱</span>
                                        <span class="font-label">餐點現點現做，請耐心等候</span>
                                    </div>
                                    <div class="ol-reminder-item">
                                        <span class="ol-reminder-icon">🍱</span>
                                        <span class="font-label"
                                            >請於完成後 15 分鐘內取餐，以確保最佳風味</span
                                        >
                                    </div>
                                    <div class="ol-reminder-item">
                                        <span class="ol-reminder-icon">🪙</span>
                                        <span class="font-label"
                                            >取餐時請告知訂單編號或出示本頁面</span
                                        >
                                    </div>
                                </div>

                                <!-- 預計取餐時間 + 顧客資訊 -->
                                <div v-if="selectedOrder.pickupTime" class="ol-pickup-banner">
                                    <div class="ol-pickup-top">
                                        <span class="font-label ol-pickup-label">預計取餐時間</span>
                                        <span class="font-headline ol-pickup-time"
                                            >今日 {{ selectedOrder.pickupTime }}</span
                                        >
                                    </div>
                                    <div class="ol-pickup-meta">
                                        <div v-if="selectedOrder.customerName" class="ol-meta-row">
                                            <span class="font-label ol-meta-label-dim">取餐人</span>
                                            <span class="font-label ol-meta-val">{{
                                                selectedOrder.customerName
                                            }}</span>
                                        </div>
                                        <div v-if="selectedOrder.customerPhone" class="ol-meta-row">
                                            <span class="font-label ol-meta-label-dim"
                                                >聯絡電話</span
                                            >
                                            <span class="font-label ol-meta-val">{{
                                                selectedOrder.customerPhone
                                            }}</span>
                                        </div>
                                        <div class="ol-meta-row">
                                            <span class="font-label ol-meta-label-dim"
                                                >付款方式</span
                                            >
                                            <span class="font-label ol-meta-val">現場付款</span>
                                        </div>
                                        <div class="ol-meta-row">
                                            <span class="font-label ol-meta-label-dim"
                                                >取餐方式</span
                                            >
                                            <span class="font-label ol-meta-val">臨櫃自取</span>
                                        </div>
                                    </div>
                                </div>

                                <!-- 已取消：大型取消卡片 -->
                                <div v-if="selectedOrder.orderStatus === 2" class="ol-cancel-card">
                                    <div class="ol-cancel-icon">✕</div>
                                    <div class="font-headline ol-cancel-title">訂單已取消</div>
                                    <p class="font-body ol-cancel-desc">
                                        此訂單已取消，餐點不會進行製作。<br />如有疑問請聯繫餐廳。
                                    </p>
                                    <a href="tel:0223456789" class="font-body ol-cancel-tel">
                                        Tel: (02) 2345-6789
                                    </a>
                                </div>

                                <!-- 修改取餐資訊按鈕（靠底部） -->
                                <div
                                    v-if="selectedOrder.orderStatus === 0"
                                    class="ol-edit-pickup-wrap"
                                >
                                    <button
                                        class="ol-edit-pickup-btn font-label"
                                        @click="showEditModal = true"
                                    >
                                        <svg
                                            xmlns="http://www.w3.org/2000/svg"
                                            width="14"
                                            height="14"
                                            fill="currentColor"
                                            viewBox="0 0 16 16"
                                        >
                                            <path
                                                d="M12.146.146a.5.5 0 0 1 .708 0l3 3a.5.5 0 0 1 0 .708l-10 10a.5.5 0 0 1-.168.11l-5 2a.5.5 0 0 1-.65-.65l2-5a.5.5 0 0 1 .11-.168l10-10zM11.207 2.5 13.5 4.793 14.793 3.5 12.5 1.207zm1.586 3L10.5 3.207 4 9.707V10h.5a.5.5 0 0 1 .5.5v.5h.5a.5.5 0 0 1 .5.5v.5h.293zm-9.761 5.175-.106.106-1.528 3.821 3.821-1.528.106-.106A.5.5 0 0 1 5 12.5V12h-.5a.5.5 0 0 1-.5-.5V11h-.5a.5.5 0 0 1-.468-.325z"
                                            />
                                        </svg>
                                        修改取餐資訊
                                    </button>
                                </div>
                            </div>
                        </div>
                    </template>
                </div>
            </div>
        </div>
    </div>

    <EditPickupModal
        :visible="showEditModal"
        :order="selectedOrder"
        :isSaving="editSaving"
        @close="showEditModal = false"
        @saved="handlePickupSaved"
    />

    <!-- ══ 餐點已完成 Modal ══ -->
    <Teleport to="body">
        <div v-if="showReadyModal" class="ol-ready-overlay">
            <div class="ol-ready-modal">
                <div class="ol-ready-icon">🍽</div>
                <h2 class="font-headline ol-ready-title">餐點已完成！</h2>
                <p class="font-body ol-ready-body">您的餐點已備妥，請至櫃台取餐並完成付款。</p>
                <button class="ol-ready-btn font-label" @click="showReadyModal = false">
                    確認
                </button>
            </div>
        </div>
    </Teleport>
</template>

<script setup>
import EditPickupModal from '@/components/order/EditPickupModal.vue'
import { ref, computed, watch, onUnmounted } from 'vue'
import apiFetch from '@/utils/apiFetch'

const lkOrderNum = ref('')
const lkName = ref('')
const lkPhone = ref('')
const searching = ref(false)
const searched = ref(false)
const error = ref('')
const results = ref([])
const selectedOrder = ref(null)
const showReadyModal = ref(false)

// ── 備註清理：舊格式含取餐資訊時只取真正的備註部分 ──
function displayNote(rawNote) {
    if (!rawNote) return ''
    if (/取餐時間[：:]|取餐人[：:]|餐具[：:]/.test(rawNote)) {
        const m = rawNote.match(/備註[：:]\s*(.+)/)
        return m ? m[1].trim() : ''
    }
    return rawNote
}

// 姓名必填 + 訂單編號/電話擇一必填
const canSearch = computed(() => {
    const name = lkName.value.trim()
    const num = lkOrderNum.value.trim()
    const phone = lkPhone.value.trim()
    return name.length > 0 && (num.length > 0 || phone.length > 0)
})

async function doLookup() {
    if (!canSearch.value || searching.value) return
    searching.value = true
    error.value = ''
    results.value = []
    selectedOrder.value = null
    searched.value = false

    try {
        // 優先用訂單編號或電話查詢
        const type = lkOrderNum.value.trim() ? 'orderNumber' : 'phone'
        const q = lkOrderNum.value.trim() || lkPhone.value.trim()

        const res = await apiFetch(
            `/Orders/Lookup?type=${encodeURIComponent(type)}&q=${encodeURIComponent(q)}`
        )
        if (!res.ok) throw new Error(`HTTP ${res.status}`)
        const all = await res.json()

        // 核對姓名（至少兩欄位都符合才顯示）
        const inputName = lkName.value.trim().toLowerCase()
        results.value = all.filter((r) => (r.customerName ?? '').toLowerCase() === inputName)
        selectedOrder.value = results.value.length > 0 ? results.value[0] : null
    } catch {
        error.value = '查詢失敗，請稍後再試'
    } finally {
        searching.value = false
        searched.value = true
    }
}

function fillDemo() {
    lkName.value     = '陳怡伶'
    lkPhone.value    = '0912111001'
    lkOrderNum.value = ''
}

function resetSearch() {
    lkOrderNum.value = ''
    lkName.value = ''
    lkPhone.value = ''
    error.value = ''
    results.value = []
    searched.value = false
    selectedOrder.value = null
}

function openEditModal() {
    // TODO：開啟編輯取餐資料 Modal
}

// ── 輪詢：當查詢結果為 status=0（進行中）時，每 30 秒刷新一次 ──
let _pollTimer = null

function startLookupPolling(orderNum) {
    stopLookupPolling()
    _pollTimer = setInterval(async () => {
        const cur = selectedOrder.value
        if (!cur || cur.orderNumber !== orderNum) {
            stopLookupPolling()
            return
        }
        if (cur.orderStatus === 2 || cur.orderStatus === 3) {
            stopLookupPolling()
            return
        }
        try {
            const res = await apiFetch(
                `/Orders/TakeoutStatus?orderNumber=${encodeURIComponent(orderNum)}`
            )
            if (!res.ok) return
            const data = await res.json()

            const updateStatus = (s) => {
                selectedOrder.value = { ...selectedOrder.value, orderStatus: s }
                const idx = results.value.findIndex((r) => r.orderNumber === orderNum)
                if (idx >= 0) results.value[idx] = { ...results.value[idx], orderStatus: s }
            }

            if (data.status === 1) {
                // 餐點已完成（待取餐）→ 彈 Modal，停止輪詢
                stopLookupPolling()
                updateStatus(1)
                showReadyModal.value = true
            } else if (data.status === 2) {
                stopLookupPolling()
                updateStatus(2)
            } else if (data.status === 3) {
                stopLookupPolling()
                updateStatus(3)
            }
            // status === 0 → 繼續輪詢
        } catch {
            // 靜默忽略
        }
    }, 30000)
}

function stopLookupPolling() {
    if (_pollTimer !== null) {
        clearInterval(_pollTimer)
        _pollTimer = null
    }
}

// 當切換到不同訂單時，重新決定是否啟動輪詢
watch(selectedOrder, (order) => {
    stopLookupPolling()
    if (order && (order.orderStatus === 0 || order.orderStatus === 1)) {
        startLookupPolling(order.orderNumber)
    }
})

onUnmounted(() => {
    stopLookupPolling()
})

const showEditModal = ref(false)
const editSaving    = ref(false)

async function handlePickupSaved(data) {
    editSaving.value = true
    try {
        const res = await apiFetch('/Orders/UpdatePickupInfo', {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data),
        })
        if (!res.ok) throw new Error()
        // 同步更新畫面
        selectedOrder.value = {
            ...selectedOrder.value,
            pickupTime:    data.pickupTime,
            customerName:  data.customerName,
            customerPhone: data.customerPhone,
        }
        // 同步更新 results 列表中對應的訂單
        const idx = results.value.findIndex((r) => r.orderNumber === data.orderNumber)
        if (idx >= 0) {
            results.value[idx] = {
                ...results.value[idx],
                pickupTime:    data.pickupTime,
                customerName:  data.customerName,
                customerPhone: data.customerPhone,
            }
        }
        showEditModal.value = false
    } catch {
        alert('儲存失敗，請稍後再試')
    } finally {
        editSaving.value = false
    }
}
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Noto+Serif+TC:wght@400;700&family=Newsreader:ital,wght@0,400;0,600;1,400&family=Work+Sans:wght@300;400&display=swap');

/* ── 頁面 ── */
.ol-page {
    min-height: 88vh;
    background: #1e100b;
    color: #f9ddd3;
    font-family: 'Newsreader', serif;
}

.ol-container {
    padding: 2rem 1.5rem;
    height: calc(100vh - 72px);
    box-sizing: border-box;
    display: flex;
    flex-direction: column;
}

/* ── 兩卡並排 ── */
.ol-cards-wrap {
    display: flex;
    gap: 1.25rem;
    flex: 1;
    min-height: 0;
}

/* ── 卡片基礎 ── */
.ol-card {
    background: #271813;
    border: 1px solid rgba(77, 70, 58, 0.4);
    border-radius: 0.85rem;
    padding: 1.5rem;
    display: flex;
    flex-direction: column;
    overflow: hidden;
}

.ol-card-title {
    font-size: 1.2rem;
    font-style: italic;
    color: #e3c76b;
    margin: 0;
    flex-shrink: 0;
}

.ol-card-header {
    display: flex;
    align-items: baseline;
    gap: 0.75rem;
    flex-shrink: 0;
}

.ol-count {
    font-size: 0.75rem;
    color: rgba(208, 197, 181, 0.4);
    letter-spacing: 0.06em;
}

.ol-card-divider {
    height: 1px;
    background: rgba(77, 70, 58, 0.4);
    margin: 0.85rem 0;
    flex-shrink: 0;
}

/* ══ 左卡：搜尋 ══ */
.ol-search-card {
    width: 450px;
    flex-shrink: 0;
    gap: 0;
}

.ol-field {
    display: flex;
    flex-direction: column;
    gap: 0.3rem;
    margin-bottom: 0.85rem;
}

.ol-label {
    font-size: 0.9rem;
    letter-spacing: 0.08em;
    color: rgba(201, 188, 168, 0.5);
    display: flex;
    align-items: center;
    gap: 0.3rem;
}

.ol-input {
    background: rgba(24, 11, 6, 0.7);
    border: 1px solid rgba(77, 70, 58, 0.45);
    border-radius: 0.3rem;
    padding: 0.55rem 0.75rem;
    color: #f9ddd3;
    font-size: 0.9rem;
    outline: none;
    width: 100%;
    box-sizing: border-box;
    transition: border-color 0.2s;
}
.ol-input:focus {
    border-color: rgba(227, 199, 107, 0.5);
}
.ol-input::placeholder {
    color: rgba(208, 197, 181, 0.25);
    font-size: 0.78rem;
}

.ol-error {
    font-size: 0.78rem;
    color: #e07070;
    margin: 0 0 0.75rem;
    text-align: center;
}

/* 展示按鈕 */
.ol-demo-btn {
    width: 100%;
    padding: 0.5rem;
    margin-bottom: 0.5rem;
    background: rgba(227, 199, 107, 0.08);
    border: 1px dashed rgba(227, 199, 107, 0.4);
    border-radius: 0.3rem;
    color: rgba(227, 199, 107, 0.7);
    font-size: 0.78rem;
    letter-spacing: 0.12em;
    cursor: pointer;
    transition: background 0.2s, border-color 0.2s, color 0.2s;
}
.ol-demo-btn:hover {
    background: rgba(227, 199, 107, 0.14);
    border-color: rgba(227, 199, 107, 0.65);
    color: #e3c76b;
}

.ol-btn-group {
    display: flex;
    flex-direction: row;
    gap: 0.5rem;
    padding-top: 0.5rem;
}
.ol-sidebar-result-label {
    font-size: 1rem;
    letter-spacing: 0.1em;
    color: rgba(208, 197, 181, 0.45);
    margin: 0.25rem 0 0.4rem;
}
.ol-sidebar-order-list {
    display: flex;
    flex-direction: column;
    gap: 0.35rem;
    max-height: 260px;
    overflow-y: auto;
    padding-right: 2px;
}
.ol-sidebar-order-item {
    width: 100%;
    text-align: left;
    padding: 0.55rem 0.8rem;
    background: rgba(10, 4, 2, 0.35);
    border: 1px solid rgba(77, 70, 58, 0.35);
    border-radius: 0.3rem;
    color: rgba(208, 197, 181, 0.75);
    font-size: 0.85rem;
    cursor: pointer;
    transition:
        border-color 0.2s,
        color 0.2s,
        background 0.2s;
    letter-spacing: 0.03em;
}
.ol-sidebar-order-item:hover {
    border-color: rgba(227, 199, 107, 0.5);
    color: #e3c76b;
    background: rgba(227, 199, 107, 0.06);
}
.ol-sidebar-order-item.active {
    border-color: #e3c76b;
    color: #e3c76b;
    background: rgba(227, 199, 107, 0.1);
}

.ol-search-btn {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 0.4rem;
    width: 100%;
    padding: 0.7rem;
    background: linear-gradient(135deg, #e3c76b, #c6ab53);
    color: #3b2f00;
    border: none;
    border-radius: 0.3rem;
    font-size: 0.9rem;
    font-weight: 600;
    letter-spacing: 0.15em;
    cursor: pointer;
    transition:
        filter 0.2s,
        opacity 0.2s;
}
.ol-search-btn:disabled {
    opacity: 0.4;
    cursor: not-allowed;
}
.ol-search-btn:not(:disabled):hover {
    filter: brightness(1.08);
}

.ol-reset-btn {
    width: 100%;
    padding: 0.6rem;
    background: transparent;
    border: 1px solid rgba(77, 70, 58, 0.4);
    border-radius: 0.3rem;
    color: rgba(208, 197, 181, 0.5);
    font-size: 1rem;
    letter-spacing: 0.1em;
    cursor: pointer;
    transition:
        border-color 0.2s,
        color 0.2s;
}
.ol-reset-btn:hover {
    border-color: rgba(208, 197, 181, 0.35);
    color: rgba(208, 197, 181, 0.75);
}

.ol-spinner {
    width: 12px;
    height: 12px;
    border: 2px solid rgba(59, 47, 0, 0.3);
    border-top-color: #3b2f00;
    border-radius: 50%;
    animation: ol-spin 0.6s linear infinite;
    display: inline-block;
}
@keyframes ol-spin {
    to {
        transform: rotate(360deg);
    }
}

/* ══ 右卡：結果 ══ */
.ol-results-card {
    flex: 1;
    min-width: 0;
    overflow-y: auto;
}

/* 空狀態 */
.ol-empty {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 0.6rem;
    padding: 4rem 2rem;
    text-align: center;
    flex: 1;
}
.ol-empty-icon {
    font-size: 2.2rem;
}
.ol-empty-text {
    font-size: 0.95rem;
    color: rgba(208, 197, 181, 0.5);
    margin: 0;
}
.ol-empty-hint {
    font-size: 0.78rem;
    color: rgba(208, 197, 181, 0.35);
    margin: 0;
    text-align: center;
}

/* loading dots */
.ol-loading-dots {
    display: flex;
    gap: 6px;
}
.ol-loading-dots span {
    width: 8px;
    height: 8px;
    border-radius: 50%;
    background: rgba(227, 199, 107, 0.5);
    animation: ol-dot 1.2s ease-in-out infinite;
}
.ol-loading-dots span:nth-child(2) {
    animation-delay: 0.2s;
}
.ol-loading-dots span:nth-child(3) {
    animation-delay: 0.4s;
}
@keyframes ol-dot {
    0%,
    80%,
    100% {
        transform: scale(0.7);
        opacity: 0.4;
    }
    40% {
        transform: scale(1);
        opacity: 1;
    }
}

/* ── 表格 ── */
.ol-table-head,
.ol-table-row {
    display: grid;
    grid-template-columns: 1fr 160px 110px 100px 90px 60px;
    align-items: center;
    gap: 0.75rem;
    padding: 0 0.5rem;
}

.ol-table-head {
    font-size: 0.72rem;
    letter-spacing: 0.1em;
    color: rgba(208, 197, 181, 0.4);
    padding-bottom: 0.65rem;
    border-bottom: 1px solid rgba(77, 70, 58, 0.3);
    margin-bottom: 0.15rem;
    flex-shrink: 0;
}

.ol-row-wrap {
    border-bottom: 1px solid rgba(77, 70, 58, 0.15);
}
.ol-row-wrap:last-child {
    border-bottom: none;
}

.ol-table-row {
    padding-top: 0.7rem;
    padding-bottom: 0.7rem;
    cursor: pointer;
    border-radius: 0.35rem;
    transition: background 0.15s;
}
.ol-table-row:hover {
    background: rgba(77, 70, 58, 0.12);
}
.ol-table-row.is-expanded {
    background: rgba(77, 70, 58, 0.18);
}

.ol-order-num {
    font-size: 0.85rem;
    color: #e3c76b;
    letter-spacing: 0.04em;
    font-family: 'Work Sans', sans-serif;
}

.ol-cell-muted {
    font-size: 0.82rem;
    color: rgba(208, 197, 181, 0.5);
}

.ol-amount {
    font-size: 0.9rem;
    color: #d5b478;
}

.ol-detail-btn {
    background: transparent;
    border: 1px solid rgba(77, 70, 58, 0.5);
    border-radius: 0.25rem;
    color: rgba(208, 197, 181, 0.6);
    font-size: 0.75rem;
    padding: 0.28rem 0.55rem;
    cursor: pointer;
    white-space: nowrap;
    transition: all 0.2s;
}
.ol-detail-btn:hover,
.ol-detail-btn.active {
    border-color: rgba(227, 199, 107, 0.5);
    color: #e3c76b;
}

/* ── 展開詳情 ── */
.ol-expand-enter-active {
    transition: all 0.28s ease;
}
.ol-expand-leave-active {
    transition: all 0.2s ease;
}
.ol-expand-enter-from,
.ol-expand-leave-to {
    opacity: 0;
    transform: translateY(-6px);
}

.ol-detail {
    background: rgba(24, 11, 6, 0.35);
    border-top: 1px solid rgba(77, 70, 58, 0.2);
    border-radius: 0 0 0.4rem 0.4rem;
    padding: 1.25rem 1.5rem;
    display: flex;
    flex-direction: column;
    gap: 1rem;
}

/* 左右兩欄 */
.ol-detail-cols {
    display: flex;
    gap: 1.25rem;
    align-items: stretch;
}
.ol-detail-left {
    flex: 1;
    min-width: 0;
    background: rgba(10, 4, 2, 0.4);
    border: 1px solid rgba(77, 70, 58, 0.3);
    border-radius: 0.5rem;
    padding: 1rem 1.1rem;
    display: flex;
    flex-direction: column;
    gap: 0;
    height: 57vh;
    overflow: hidden; /* 整體不捲動 */
}
.ol-detail-right {
    width: 480px;
    flex-shrink: 0;
    display: flex;
    flex-direction: column;
    gap: 0.75rem;
}
.ol-section-label {
    font-size: 0.9rem;
    letter-spacing: 0.12em;
    color: rgba(208, 197, 181, 0.4);
    margin-bottom: 0.65rem;
    text-transform: uppercase;
}

/* 提示訊息 */
.ol-tips {
    display: flex;
    flex-direction: column;
    gap: 0.45rem;
}
.ol-tip {
    display: flex;
    align-items: flex-start;
    gap: 0.55rem;
    font-size: 1rem;
    color: rgba(208, 197, 181, 0.7);
    line-height: 1.4;
}
.ol-tip-icon {
    flex-shrink: 0;
    font-size: 0.9rem;
    margin-top: 0.05rem;
}

/* 顧客資訊 */
.ol-info-rows {
    display: flex;
    flex-direction: column;
    gap: 0.3rem;
}
.ol-info-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    font-size: 0.86rem;
    padding: 0.25rem 0;
    border-bottom: 1px solid rgba(77, 70, 58, 0.15);
}
.ol-info-row:last-child {
    border-bottom: none;
}
.ol-info-label {
    color: rgba(208, 197, 181, 0.5);
    font-size: 0.82rem;
}
.ol-info-val {
    color: #f9ddd3;
    font-size: 0.88rem;
    text-align: right;
}

/* 編輯取餐資料按鈕 */
.ol-edit-btn {
    width: 100%;
    padding: 0.65rem;
    background: transparent;
    border: 1px solid rgba(227, 199, 107, 0.45);
    border-radius: 0.3rem;
    color: #e3c76b;
    font-size: 0.85rem;
    letter-spacing: 0.12em;
    cursor: pointer;
    transition:
        background 0.2s,
        border-color 0.2s;
    margin-top: 0.25rem;
}
.ol-edit-btn:hover {
    background: rgba(227, 199, 107, 0.08);
    border-color: rgba(227, 199, 107, 0.7);
}

/* 進度條 */
.ol-progress {
    margin: 1rem 0;
    width: 100%;
    display: flex;
    align-items: center;
}
.ol-prog-step {
    display: flex;
    flex-direction: row;
    align-items: center;
    gap: 0.55rem;
    flex: 0 0 auto;
}
.ol-prog-dot {
    width: 35px;
    height: 35px;
    border-radius: 50%;
    border: 2px solid rgba(77, 70, 58, 0.4);
    background: #1e100b;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 1rem;
    color: rgba(208, 197, 181, 0.4);
    transition: all 0.4s;
    flex-shrink: 0;
}
.ol-prog-done .ol-prog-dot {
    background: linear-gradient(135deg, #e3c76b, #c6ab53);
    border-color: #e3c76b;
    color: #3b2f00;
}
.ol-prog-active .ol-prog-dot {
    border-color: #e3c76b;
    background: rgba(227, 199, 107, 0.1);
    color: #e3c76b;
    animation: ol-pulse 1.4s ease-in-out infinite;
}
@keyframes ol-pulse {
    0%,
    100% {
        box-shadow: 0 0 0 0 rgba(227, 199, 107, 0.4);
    }
    50% {
        box-shadow: 0 0 0 8px rgba(227, 199, 107, 0);
    }
}
.ol-prog-lbl {
    font-size: 1rem;
    letter-spacing: 0.05em;
    color: rgba(208, 197, 181, 0.4);
    white-space: nowrap;
}
.ol-prog-done .ol-prog-lbl,
.ol-prog-active .ol-prog-lbl {
    color: #e3c76b;
}
.ol-prog-line {
    margin: 0 1rem;
    flex: 1;
    height: 2px;
    background: rgba(77, 70, 58, 0.4);
}
.ol-prog-line-lit {
    background: linear-gradient(90deg, #e3c76b, #c6ab53);
}

/* 預計取餐 */
.ol-pickup-banner {
    background: rgba(24, 11, 6, 0.5);
    border: 1px solid rgba(227, 199, 107, 0.2);
    border-radius: 0.4rem;
    padding: 0.75rem 1rem;
    display: flex;
    flex-direction: column;
    align-items: stretch;
    gap: 0.6rem;
    width: 100%;
}
.ol-pickup-top {
    display: flex;
    flex-direction: column;
    gap: 0.2rem;
}
.ol-pickup-label {
    font-size: 0.72rem;
    letter-spacing: 0.1em;
    color: rgba(208, 197, 181, 0.4);
    text-transform: uppercase;
}
.ol-pickup-time {
    font-size: 1.35rem;
    color: #e3c76b;
    letter-spacing: 0.04em;
}
.ol-pickup-meta {
    border-top: 1px solid rgba(77, 70, 58, 0.3);
    padding-top: 0.55rem;
    display: flex;
    flex-direction: column;
    gap: 0.35rem;
}

/* 品項 */
.ol-items-detail {
    display: flex;
    flex-direction: column;
    flex: 1; /* 撐滿剩餘空間 */
    min-height: 0; /* flex 子元素縮小必要條件 */
    overflow-y: auto;
    /* 捲軸樣式（可選） */
    scrollbar-width: thin;
    scrollbar-color: rgba(77, 70, 58, 0.4) transparent;
}
.ol-item {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    gap: 1rem;
    padding: 0.48rem 0;
    border-bottom: 1px solid rgba(77, 70, 58, 0.12);
}
.ol-item:last-child {
    border-bottom: none;
}
.ol-item-left {
    flex: 1;
}
.ol-item-right {
    display: flex;
    flex-direction: column;
    align-items: flex-end;
    gap: 0.1rem;
    flex-shrink: 0;
}
.ol-item-name {
    font-size: 1rem;
    color: #f9ddd3;
}
.ol-item-note {
    font-size: 0.73rem;
    color: rgba(208, 197, 181, 0.4);
    display: block;
    margin-top: 0.1rem;
}
.ol-subitems {
    display: flex;
    flex-wrap: wrap;
    gap: 0.25rem;
    margin-top: 0.2rem;
}
.ol-subitem {
    font-size: 0.68rem;
    color: rgba(208, 197, 181, 0.5);
    background: rgba(77, 70, 58, 0.25);
    padding: 0.08rem 0.38rem;
    border-radius: 999px;
}
.setmeal-badge-sm {
    font-size: 0.65rem;
    color: #e3c76b;
    margin-bottom: 0.08rem;
}

/* 金額 Meta */
.ol-meta-rows {
    display: flex;
    flex-direction: column;
    gap: 0.42rem;
    flex-shrink: 0; /* 金額區塊固定，不被壓縮 */
}
.ol-meta-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    font-size: 0.86rem;
    flex-shrink: 0; /* 各金額列固定，不被壓縮 */
}
/* 三欄列：標籤 | 名稱（彈性） | 金額 */
.ol-meta-row-3 {
    display: flex;
    align-items: center;
    gap: 0.45rem;
    font-size: 0.86rem;
}
.ol-meta-row-3 .ol-meta-label {
    flex-shrink: 0;
    min-width: 2.8rem;
    color: rgba(208, 197, 181, 0.5);
}
.ol-meta-val-mid {
    flex: 1;
    color: #f9ddd3;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}
.ol-meta-discount {
    flex-shrink: 0;
    color: #7ec87e;
}
.ol-meta-label-dim {
    color: rgba(208, 197, 181, 0.5);
}
.ol-meta-val {
    color: #f9ddd3;
}
/* 贈品背景 */
.ol-item-gift {
    background: rgba(163, 217, 119, 0.04);
}
/* 已結帳提示 */
.ol-paid-badge {
    display: inline-flex;
    align-items: center;
    gap: 0.3rem;
    font-size: 0.8rem;
    color: #7ec87e;
    background: rgba(126, 200, 126, 0.08);
    border: 1px solid rgba(126, 200, 126, 0.25);
    border-radius: 0.3rem;
    padding: 0.3rem 0.7rem;
    margin-bottom: 0.5rem;
}
/* 已取消提示 */
.ol-cancelled-badge {
    display: inline-flex;
    align-items: center;
    gap: 0.3rem;
    font-size: 0.8rem;
    color: #e07070;
    background: rgba(224, 112, 112, 0.08);
    border: 1px solid rgba(224, 112, 112, 0.25);
    border-radius: 0.3rem;
    padding: 0.3rem 0.7rem;
    margin-bottom: 0.5rem;
}
/* 贈品活動來源標註 */
.ol-gift-event-note {
    font-size: 0.76rem;
    color: rgba(126, 200, 126, 0.75);
    padding: 0.2rem 0.4rem;
    letter-spacing: 0.03em;
}

/* feather-divider */
.feather-divider {
    height: 1px;
    background: linear-gradient(90deg, transparent, #e4c285 50%, transparent);
    position: relative;
    max-width: 460px;
    flex-shrink: 0; /* 分隔線固定，不被壓縮 */
}
.feather-divider::after {
    content: '◈';
    position: absolute;
    left: 50%;
    top: 50%;
    transform: translate(-50%, -50%);
    color: #e4c285;
    background: #1e100b;
    padding: 0 0.3rem;
    font-size: 0.65rem;
}

/* ── 訂單編號 header ── */
.ol-order-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: 0.25rem;
}
.ol-order-num-block {
    display: flex;
    flex-direction: row;
    gap: 0.1rem;
}
.ol-order-num-label {
    font-size: 0.72rem;
    letter-spacing: 0.1em;
    color: rgba(208, 197, 181, 0.4);
    text-transform: uppercase;
}
.ol-order-num-val {
    font-size: 1rem;
    color: #e3c76b;
    letter-spacing: 0.04em;
    font-family: 'Work Sans', sans-serif;
}

/* ── Section title ── */
.ol-section-title {
    font-size: 0.72rem;
    letter-spacing: 0.12em;
    color: rgba(208, 197, 181, 0.4);
    text-transform: uppercase;
    margin: 0 0 0.65rem;
    flex-shrink: 0; /* 標題不縮，固定在頂部 */
}

/* ── 防呆提醒 ── */
.ol-reminders {
    background: rgba(10, 4, 2, 0.4);
    border: 1px solid rgba(77, 70, 58, 0.3);
    border-radius: 0.5rem;
    padding: 0.85rem 1rem;
    display: flex;
    flex-direction: column;
    gap: 0.55rem;
}
.ol-reminder-item {
    display: flex;
    align-items: flex-start;
    gap: 0.55rem;
    font-size: 0.83rem;
    color: rgba(208, 197, 181, 0.7);
    line-height: 1.45;
}
.ol-reminder-icon {
    flex-shrink: 0;
    font-size: 0.85rem;
    margin-top: 0.05rem;
}

/* ── 聯絡資訊 ── */
.ol-contact-section {
    background: rgba(10, 4, 2, 0.4);
    border: 1px solid rgba(77, 70, 58, 0.3);
    border-radius: 0.5rem;
    padding: 0.85rem 1rem;
}
.ol-store-addr {
    font-size: 0.85rem;
    color: rgba(208, 197, 181, 0.6);
    margin: 0 0 0.35rem;
}
.ol-store-tel {
    font-size: 0.85rem;
    color: #e3c76b;
    text-decoration: none;
    letter-spacing: 0.02em;
    transition: color 0.2s;
    display: block;
}
.ol-store-tel:hover {
    color: #f0d87a;
}

/* ── item qty / price ── */
.ol-item-qty {
    font-size: 0.82rem;
    color: rgba(208, 197, 181, 0.5);
}
.ol-item-price {
    font-size: 0.9rem;
    color: #d5b478;
    white-space: nowrap;
}

/* 多筆切換 tab */
.ol-tabs {
    display: flex;
    flex-wrap: wrap;
    gap: 0.4rem;
    margin-bottom: 0.25rem;
    flex-shrink: 0;
}
.ol-tab {
    background: transparent;
    border: 1px solid rgba(77, 70, 58, 0.5);
    border-radius: 0.25rem;
    color: rgba(208, 197, 181, 0.55);
    font-size: 1rem;
    padding: 0.3rem 0.75rem;
    cursor: pointer;
    transition: all 0.2s;
    font-family: 'Work Sans', sans-serif;
    letter-spacing: 0.04em;
}
.ol-tab:hover {
    border-color: rgba(227, 199, 107, 0.4);
    color: rgba(208, 197, 181, 0.85);
}
.ol-tab.active {
    border-color: #e3c76b;
    color: #e3c76b;
    background: rgba(227, 199, 107, 0.08);
}

/* ── 手機版 ── */
@media (max-width: 768px) {
    .ol-container {
        padding: 1rem;
        height: auto;
    }
    .ol-cards-wrap {
        flex-direction: column;
    }
    .ol-search-card {
        width: 100%;
    }
    .ol-results-card {
        overflow-y: visible;
    }
    .ol-table-head {
        display: none;
    }
    .ol-table-row {
        display: flex;
        flex-wrap: wrap;
        gap: 0.35rem 0.75rem;
        align-items: center;
    }
    .col-phone {
        display: none;
    }
    .ol-detail-cols {
        flex-direction: column;
    }
    .ol-detail-right {
        width: 100%;
    }
}

/* ── 取消狀態：進度條全暗 ── */
.ol-progress-cancelled .ol-prog-dot {
    background: #1e100b !important;
    border-color: rgba(77, 70, 58, 0.3) !important;
    color: rgba(208, 197, 181, 0.2) !important;
}
.ol-progress-cancelled .ol-prog-lbl {
    color: rgba(208, 197, 181, 0.2) !important;
}

/* ── 完成狀態：置中單一 badge ── */
.ol-progress-done-single {
    margin: 1rem 0;
    width: 100%;
    display: flex;
    justify-content: center;
    align-items: center;
}
.ol-done-single-badge {
    display: inline-flex;
    align-items: center;
    gap: 0.55rem;
    background: linear-gradient(135deg, rgba(227, 199, 107, 0.15), rgba(198, 171, 83, 0.1));
    border: 1.5px solid rgba(227, 199, 107, 0.45);
    border-radius: 2rem;
    padding: 0.6rem 1.5rem;
    color: #e3c76b;
    font-size: 1.05rem;
    letter-spacing: 0.08em;
}
.ol-done-single-badge svg {
    color: #e3c76b;
    flex-shrink: 0;
}
/* 已結帳：改綠色 */
.ol-done-paid-badge {
    background: linear-gradient(135deg, rgba(126, 200, 126, 0.15), rgba(100, 170, 100, 0.1));
    border-color: rgba(126, 200, 126, 0.45);
    color: #7ec87e;
}
.ol-done-paid-badge svg {
    color: #7ec87e;
}

/* ── 修改取餐資訊按鈕 ── */
.ol-edit-pickup-wrap {
    display: flex;
    justify-content: center;
    margin-top: auto;
    padding-top: 1rem;
}
.ol-edit-pickup-btn {
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.65rem 1.4rem;
    background: transparent;
    border: 1.5px solid rgba(227, 199, 107, 0.45);
    border-radius: 0.4rem;
    color: #e3c76b;
    font-size: 0.92rem;
    letter-spacing: 0.08em;
    cursor: pointer;
    transition:
        background 0.2s,
        border-color 0.2s,
        filter 0.2s;
    width: 100%;
    justify-content: center;
}
.ol-edit-pickup-btn:hover {
    background: rgba(227, 199, 107, 0.1);
    border-color: rgba(227, 199, 107, 0.75);
    filter: brightness(1.1);
}
.ol-edit-pickup-btn svg {
    flex-shrink: 0;
}

/* ── 取消卡片（取代聯絡資訊卡片） ── */
.ol-cancel-card {
    background: rgba(224, 112, 112, 0.06);
    border: 1.5px solid rgba(224, 112, 112, 0.35);
    border-radius: 0.6rem;
    padding: 1.25rem 1.1rem;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 0.55rem;
    text-align: center;
}
.ol-cancel-icon {
    font-size: 1.8rem;
    color: #e07070;
    font-weight: 700;
    line-height: 1;
}
.ol-cancel-title {
    font-size: 1.15rem;
    color: #e07070;
    letter-spacing: 0.06em;
    margin: 0;
}
.ol-cancel-desc {
    font-size: 0.83rem;
    color: rgba(208, 197, 181, 0.6);
    margin: 0;
    line-height: 1.6;
}
.ol-cancel-tel {
    font-size: 0.85rem;
    color: rgba(224, 112, 112, 0.7);
    text-decoration: none;
    letter-spacing: 0.02em;
    transition: color 0.2s;
}
.ol-cancel-tel:hover {
    color: #e07070;
}

/* ── 餐點已完成 Modal ── */
.ol-ready-overlay {
    position: fixed;
    inset: 0;
    background: rgba(10, 4, 2, 0.82);
    backdrop-filter: blur(4px);
    z-index: 9800;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 1.5rem;
}
.ol-ready-modal {
    background: #271813;
    border: 1px solid rgba(227, 199, 107, 0.35);
    border-radius: 1rem;
    padding: 2.5rem 2rem;
    max-width: 400px;
    width: 100%;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 1rem;
    box-shadow: 0 8px 48px rgba(0, 0, 0, 0.6);
    animation: ol-rm-pop 0.28s cubic-bezier(0.34, 1.56, 0.64, 1);
}
@keyframes ol-rm-pop {
    from {
        opacity: 0;
        transform: scale(0.88);
    }
    to {
        opacity: 1;
        transform: scale(1);
    }
}
.ol-ready-icon {
    font-size: 2.8rem;
}
.ol-ready-title {
    font-size: 1.6rem;
    color: #e3c76b;
    margin: 0;
    letter-spacing: 0.05em;
}
.ol-ready-body {
    font-size: 0.95rem;
    color: rgba(208, 197, 181, 0.75);
    text-align: center;
    line-height: 1.6;
    margin: 0;
}
.ol-ready-btn {
    margin-top: 0.5rem;
    width: 100%;
    padding: 0.85rem;
    background: linear-gradient(135deg, #e3c76b, #c6ab53);
    color: #3b2f00;
    border: none;
    border-radius: 0.4rem;
    font-size: 1rem;
    font-weight: 600;
    letter-spacing: 0.2em;
    cursor: pointer;
    transition: filter 0.2s;
}
.ol-ready-btn:hover {
    filter: brightness(1.1);
}
</style>
