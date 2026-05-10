<script setup>
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import apiFetch from '@/utils/apiFetch.js'
import { useToast } from '@/composables/useToast.js'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'

const { show } = useToast()

// ── 狀態 ───────────────────────────────────────────────
const orders = ref([])
const isLoading = ref(false)
const hasMore = ref(true)
const isFirstLoad = ref(true)

const currentPage = ref(1)
const totalPages = ref(1)
const PAGE_SIZE = 10

const dateFrom = ref(null)
const dateTo = ref(null)

const sentinel = ref(null)
let observer = null

// ── 展開狀態（餐點明細 / 付款明細 各自獨立） ────────────
const expandedItems = ref(new Set())
const expandedPayment = ref(new Set())

function toggleItems(orderNumber) {
    const next = new Set(expandedItems.value)
    next.has(orderNumber) ? next.delete(orderNumber) : next.add(orderNumber)
    expandedItems.value = next
}

function togglePayment(orderNumber) {
    const next = new Set(expandedPayment.value)
    next.has(orderNumber) ? next.delete(orderNumber) : next.add(orderNumber)
    expandedPayment.value = next
}

// ── 衍生狀態 ───────────────────────────────────────────
const isEmpty = computed(() => !isLoading.value && orders.value.length === 0)
const isFiltered = computed(() => dateFrom.value || dateTo.value)

// ── API 呼叫 ───────────────────────────────────────────
async function fetchOrders(page) {
    if (isLoading.value) return

    isLoading.value = true
    try {
        const params = new URLSearchParams({ page, pageSize: PAGE_SIZE })
        if (dateFrom.value) params.set('dateFrom', toISODate(dateFrom.value))
        if (dateTo.value) params.set('dateTo', toISODate(dateTo.value))

        const res = await apiFetch(`/members/me/orders?${params}`)
        if (!res.ok) {
            show('載入失敗，請重新整理', 'error')
            return
        }

        const data = await res.json()
        if (page === 1) {
            orders.value = data.items
        } else {
            orders.value.push(...data.items)
        }

        totalPages.value = data.totalPages
        currentPage.value = page
        hasMore.value = page < data.totalPages
    } catch {
        show('載入失敗，請重新整理', 'error')
    } finally {
        isLoading.value = false
        isFirstLoad.value = false
    }
}

function loadMore() {
    if (!hasMore.value || isLoading.value) return
    fetchOrders(currentPage.value + 1)
}

// ── 日期篩選：改變時重置並重新載入 ────────────────────
function resetAndFetch() {
    orders.value = []
    currentPage.value = 1
    totalPages.value = 1
    hasMore.value = true
    isFirstLoad.value = true
    expandedItems.value = new Set()
    expandedPayment.value = new Set()
    fetchOrders(1)
}

watch([dateFrom, dateTo], resetAndFetch)

// ── IntersectionObserver（無限滾動） ───────────────────
function setupObserver() {
    observer = new IntersectionObserver(
        (entries) => {
            if (entries[0].isIntersecting) loadMore()
        },
        { threshold: 0.1 }
    )
    if (sentinel.value) observer.observe(sentinel.value)
}

// ── 工具函式 ───────────────────────────────────────────
function toISODate(d) {
    const dt = d instanceof Date ? d : new Date(d)
    return dt.toISOString().split('T')[0]
}

function formatDate(isoStr) {
    const d = new Date(isoStr)
    const yyyy = d.getFullYear()
    const MM = String(d.getMonth() + 1).padStart(2, '0')
    const dd = String(d.getDate()).padStart(2, '0')
    const HH = String(d.getHours()).padStart(2, '0')
    const mm = String(d.getMinutes()).padStart(2, '0')
    return `${yyyy}/${MM}/${dd} ${HH}:${mm}`
}

function payMethodLabel(method) {
    const map = { Cash: '現金', Card: '刷卡', LinePay: '行動支付', ECPay: '線上付款' }
    return map[method] ?? method
}

function formatPrice(amount) {
    return Number(amount).toLocaleString('zh-TW')
}

// ── 生命週期 ───────────────────────────────────────────
onMounted(async () => {
    await fetchOrders(1)
    setupObserver()
})

onUnmounted(() => {
    observer?.disconnect()
})
</script>

<template>
    <div class="d-flex flex-column gap-4">
        <!-- 標題列 + 日期篩選 -->
        <div class="d-flex flex-column align-items-sm-start justify-content-between gap-3">
            <div>
                <h2 class="eat-h3 fst-normal mb-1">訂單紀錄</h2>
                <p class="eat-body-muted mb-0">查看您的歷史消費紀錄</p>
            </div>
            <div class="d-flex gap-2 align-items-center flex-wrap">
                <VueDatePicker
                    v-model="dateFrom"
                    placeholder="起始日期"
                    :enable-time-picker="false"
                    auto-apply
                    style="width: 180px"
                    :formats="{ input: 'yyyy/MM/dd' }"
                />
                <span class="eat-label" style="color: var(--eat-on-surface-variant)">—</span>
                <VueDatePicker
                    v-model="dateTo"
                    placeholder="結束日期"
                    :enable-time-picker="false"
                    auto-apply
                    style="width: 180px"
                    :formats="{ input: 'yyyy/MM/dd' }"
                />
            </div>
        </div>

        <!-- Skeleton（首次載入） -->
        <template v-if="isFirstLoad && isLoading">
            <div v-for="n in 3" :key="n" class="card-eat p-4">
                <div class="d-flex flex-column gap-2">
                    <div class="skeleton-line" style="width: 40%"></div>
                    <div class="skeleton-line" style="width: 25%"></div>
                    <div class="skeleton-line" style="width: 30%"></div>
                    <div class="skeleton-line" style="width: 18%"></div>
                </div>
            </div>
        </template>

        <!-- 空狀態 -->
        <div v-else-if="isEmpty" class="card-eat p-5 text-center">
            <p class="eat-body-muted mb-0">
                {{ isFiltered ? '查無符合條件的訂單' : '目前尚無訂單紀錄' }}
            </p>
        </div>

        <!-- 訂單卡片列表 -->
        <template v-else>
            <div v-for="order in orders" :key="order.orderNumber" class="card-eat">
                <!-- ── 卡片頂部：摘要（永遠顯示） ── -->
                <div class="p-4">
                    <!-- 第一行：訂單編號 + Badge -->
                    <div class="d-flex justify-content-between align-items-start mb-1">
                        <span class="fw-medium eat-label">訂單編號：{{ order.orderNumber }}</span>
                        <span
                            class="order-badge ms-2 flex-shrink-0"
                            :class="order.inOrOut ? 'order-badge--green' : 'order-badge--orange'"
                            >{{ order.inOrOut ? '內用' : '外帶' }}</span
                        >
                    </div>
                    <!-- 第二行：下單時間 + 實付金額 -->
                    <div class="d-flex justify-content-between align-items-center mt-1">
                        <span class="eat-body-muted small">{{ formatDate(order.orderAt) }}</span>
                        <span class="text-eat-primary fw-medium"
                            >NT$ {{ formatPrice(order.totalAmount) }}</span
                        >
                    </div>
                </div>

                <!-- ── 餐點明細 toggle ── -->
                <div
                    class="border-top d-flex justify-content-between align-items-center px-4 py-3"
                    style="cursor: pointer; user-select: none"
                    @click="toggleItems(order.orderNumber)"
                >
                    <span class="fw-semibold d-flex align-items-center gap-2">
                        <i class="bi bi-receipt"></i>餐點明細
                    </span>
                    <i
                        class="bi bi-chevron-down"
                        style="color: var(--eat-primary)"
                        :style="{
                            transform: expandedItems.has(order.orderNumber)
                                ? 'rotate(180deg)'
                                : 'rotate(0deg)',
                            transition: 'transform 0.2s',
                        }"
                    ></i>
                </div>

                <!-- 餐點明細 內容 -->
                <div v-if="expandedItems.has(order.orderNumber)">
                    <template v-if="order.items && order.items.length > 0">
                        <div
                            v-for="(item, idx) in order.items"
                            :key="idx"
                            class="d-flex justify-content-between align-items-center px-4 pb-3 small"
                        >
                            <span class="text-eat-muted"
                                >{{ item.productName }} × {{ item.qty }}</span
                            >
                            <span class="ms-3 text-nowrap">
                                ${{ formatPrice(item.unitPrice * item.qty) }}
                            </span>
                        </div>
                    </template>
                    <p v-else class="text-eat-muted text-center py-2 mb-0">無品項資訊</p>
                </div>

                <!-- ── 付款明細 toggle ── -->
                <div
                    class="border-top d-flex justify-content-between align-items-center px-4 py-3"
                    style="cursor: pointer; user-select: none"
                    @click="togglePayment(order.orderNumber)"
                >
                    <span class="fw-semibold d-flex align-items-center gap-2">
                        <i class="bi bi-credit-card"></i>付款明細
                    </span>
                    <i
                        class="bi bi-chevron-down"
                        style="color: var(--eat-primary)"
                        :style="{
                            transform: expandedPayment.has(order.orderNumber)
                                ? 'rotate(180deg)'
                                : 'rotate(0deg)',
                            transition: 'transform 0.2s',
                        }"
                    ></i>
                </div>

                <!-- 付款明細 內容 -->
                <div v-if="expandedPayment.has(order.orderNumber)" class="px-4 pb-3">
                    <!-- 付款方式 -->
                    <div class="d-flex justify-content-between py-1 small">
                        <span class="text-eat-muted">付款方式</span>
                        <span>{{ payMethodLabel(order.payMethod) }}</span>
                    </div>
                    <!-- 原始金額 -->
                    <div class="d-flex justify-content-between py-1 small">
                        <span class="text-eat-muted">小計</span>
                        <span>NT$ {{ formatPrice(order.originalAmount) }}</span>
                    </div>
                    <!-- 折扣行 -->
                    <template v-if="order.discountAmount > 0">
                        <div
                            v-if="order.couponCode"
                            class="d-flex justify-content-between py-1 small"
                        >
                            <span class="text-eat-muted">優惠券 {{ order.couponCode }}</span>
                            <span style="color: var(--eat-error)"
                                >-${{ formatPrice(order.discountAmount) }}</span
                            >
                        </div>
                        <div
                            v-if="order.eventTitle"
                            class="d-flex justify-content-between py-1 small"
                        >
                            <span class="text-eat-muted">活動 {{ order.eventTitle }}</span>
                            <span style="color: var(--eat-error)"
                                >-${{ formatPrice(order.discountAmount) }}</span
                            >
                        </div>
                        <div
                            v-if="!order.couponCode && !order.eventTitle"
                            class="d-flex justify-content-between py-1 small"
                        >
                            <span class="text-eat-muted">折扣</span>
                            <span style="color: var(--eat-error)"
                                >-${{ formatPrice(order.discountAmount) }}</span
                            >
                        </div>
                    </template>
                    <!-- 分隔線 -->
                    <hr class="my-2" />
                    <!-- 實付金額 -->
                    <div class="d-flex justify-content-between align-items-center py-1">
                        <span class="fw-semibold">實付金額</span>
                        <span class="text-eat-primary fw-semibold"
                            >NT$ {{ formatPrice(order.totalAmount) }}</span
                        >
                    </div>
                </div>
            </div>
        </template>

        <!-- 哨兵：IntersectionObserver 觀察點 -->
        <div ref="sentinel" class="sentinel"></div>

        <!-- 底部狀態 -->
        <div v-if="!isFirstLoad" class="text-center py-2">
            <LoadingSpinner v-if="isLoading" />
            <p v-else-if="!hasMore && orders.length > 0" class="eat-body-muted mb-0">
                已顯示全部訂單
            </p>
        </div>
    </div>
</template>

<style scoped>
/* ── 金額 ── */
/* .eat-price {
    font-family: var(--font-label);
    font-size: 1rem;
    color: var(--eat-primary);
    font-weight: 600;
    white-space: nowrap;
} */

/* ── 內用 / 外帶 Badge ── */
.order-badge {
    display: inline-block;
    font-size: 0.75rem;
    font-weight: 600;
    padding: 0.2em 0.65em;
    border-radius: 999px;
    white-space: nowrap;
    line-height: 1.4;
}

.order-badge--green {
    background: #d1fae5;
    color: #065f46;
}
.order-badge--orange {
    background: #ffedd5;
    color: #9a3412;
}

/* ── 哨兵（不可見） ── */
.sentinel {
    height: 1px;
}

/* ── Skeleton shimmer ── */
.skeleton-line {
    height: 0.875rem;
    border-radius: 4px;
    background: linear-gradient(
        90deg,
        var(--eat-surface) 25%,
        var(--eat-surface-container) 50%,
        var(--eat-surface) 75%
    );
    background-size: 200% 100%;
    animation: order-shimmer 1.4s infinite;
}

@keyframes order-shimmer {
    0% {
        background-position: 200% 0;
    }
    100% {
        background-position: -200% 0;
    }
}
</style>
