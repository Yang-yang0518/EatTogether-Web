<template>
    <div class="bell-wrapper">
        <!-- 鈴鐺按鈕 -->
        <button class="bell-btn" @click="toggleDropdown" title="通知">
            <i class="bi bi-bell-fill"></i>
            <span v-if="unreadCount > 0" class="bell-badge">{{ unreadCount }}</span>
        </button>

        <!-- 通知下拉清單 -->
        <Transition name="bell-dropdown">
            <div v-if="isOpen" class="bell-dropdown">
                <div class="dropdown-header">
                    <span>通知</span>
                    <button v-if="unreadCount > 0" @click="markAllAsRead">全部標記為已讀</button>
                </div>

                <ul v-if="notifications.length > 0">
                    <li
                        v-for="n in notifications"
                        :key="n.id"
                        :class="{ unread: !n.isRead }"
                        @click="handleClick(n)"
                    >
                        <div class="notification-row">
                            <span class="title">{{ n.title }}</span>
                            <span v-if="!n.isRead" class="unread-dot"></span>
                        </div>
                        <!-- message 有值才顯示 -->
                        <span v-if="n.message" class="message">{{ n.message }}</span>
                        <span class="time">{{ formatTime(n.createdAt) }}</span>
                    </li>
                </ul>

                <p v-else class="empty">目前沒有通知</p>
            </div>
        </Transition>
    </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import apiFetch from '@/utils/apiFetch.js'
import { useRouter } from 'vue-router'

const router = useRouter()

const notifications = ref([])
const isOpen = ref(false)

// 未讀數量
const unreadCount = computed(() => notifications.value.filter((n) => !n.isRead).length)

// 取得通知列表
async function fetchNotifications() {
    try {
        const res = await apiFetch('/Notifications')
        if (!res.ok) return
        notifications.value = await res.json()
    } catch (e) {
        console.error('取得通知失敗', e)
    }
}

// 標記單筆已讀 → 導向相關頁面
async function handleClick(notification) {
    if (!notification.isRead) {
        await apiFetch(`/Notifications/${notification.id}/read`, {
            method: 'PATCH',
        })
        notification.isRead = true
    }
    isOpen.value = false

    // 依通知類型決定跳轉頁面，以下註解為範例，可自訂更改
    const routes = {
        NEWS: () => router.push({ name: 'NewsDetail', params: { id: notification.referenceId } }),
        RESERVATION_CONFIRM: () => router.push({ name: 'MyReservations' }),
        RESERVATION_CANCEL: () => router.push({ name: 'MyReservations' }),
        COUPON_RECEIVED: () => router.push({ name: 'MyCoupons' }),
        TAKEOUT_CREATED: () => router.push({ name: 'OrderLookup' }),
        TAKEOUT_READY: () => router.push({ name: 'OrderLookup' }),
        // RESERVATION_REMIND: () =>
        //     router.push({ name: 'MyReservations' }),
        // COUPON_EXPIRING: () => router.push({ name: 'CouponUsage' }),
    }
    const navigate = routes[notification.type]
    if (navigate) navigate()
}

// 全部已讀
async function markAllAsRead() {
    await apiFetch('/Notifications/read-all', {
        method: 'PATCH',
    })
    notifications.value.forEach((n) => (n.isRead = true))
}

// 展開 / 收合
function toggleDropdown() {
    isOpen.value = !isOpen.value
    if (isOpen.value) fetchNotifications()
}

// 點擊外部關閉下拉
function handleOutsideClick(e) {
    if (!e.target.closest('.bell-wrapper')) {
        isOpen.value = false
    }
}

// 時間格式化
function formatTime(dateStr) {
    const date = new Date(dateStr)
    return `${date.getMonth() + 1}/${date.getDate()} ${date.getHours()}:${String(date.getMinutes()).padStart(2, '0')}`
}

onMounted(() => {
    fetchNotifications()
    document.addEventListener('click', handleOutsideClick)
})

onUnmounted(() => {
    document.removeEventListener('click', handleOutsideClick)
})
</script>

<style scoped>
/* ── 容器 ── */
.bell-wrapper {
    position: relative;
    display: inline-block;
}

/* ── 鈴鐺按鈕 ── */
.bell-btn {
    position: relative;
    background: none;
    border: none;
    cursor: pointer;
    color: var(--eat-on-surface-variant);
    transition: color 0.3s ease;
    padding: 0;
    width: 32px;
    height: 32px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 1.1rem;
}

.bell-btn:hover {
    color: var(--eat-primary);
}

/* ── 未讀 badge ── */
.bell-badge {
    position: absolute;
    top: 0;
    right: 0;
    transform: translate(40%, -40%);
    background: var(--eat-error-container);
    color: var(--eat-error);
    border-radius: var(--eat-radius-full);
    font-size: 0.6rem;
    font-family: var(--font-label);
    min-width: 16px;
    height: 16px;
    padding: 0 3px;
    display: flex;
    align-items: center;
    justify-content: center;
    pointer-events: none;
}

/* ── 下拉面板 ── */
.bell-dropdown {
    position: absolute;
    right: 0;
    top: calc(100% + 0.5rem);
    width: 300px;
    background: var(--eat-surface-container);
    backdrop-filter: blur(20px);
    border: 1px solid var(--eat-outline-variant);
    border-radius: var(--eat-radius-lg);
    box-shadow: 0 20px 50px rgba(0, 0, 0, 0.5);
    z-index: 999;
    overflow: hidden;
}

/* ── 下拉動畫 ── */
.bell-dropdown-enter-active {
    transition:
        opacity 0.15s ease,
        transform 0.15s ease;
}
.bell-dropdown-leave-active {
    transition:
        opacity 0.1s ease,
        transform 0.1s ease;
}
.bell-dropdown-enter-from,
.bell-dropdown-leave-to {
    opacity: 0;
    transform: translateY(-4px);
}

/* ── 面板標頭 ── */
.dropdown-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 0.75rem 1rem;
    border-bottom: 1px solid var(--eat-outline-variant);
    font-family: var(--font-label);
    font-size: 0.75rem;
    letter-spacing: 0.14em;
    text-transform: uppercase;
    /* 標題用次要色，比訊息文字稍暗但不會差太多 */
    color: var(--eat-secondary);
}

.dropdown-header button {
    background: none;
    border: none;
    cursor: pointer;
    font-family: var(--font-label);
    font-size: 0.7rem;
    letter-spacing: 0.1em;
    color: var(--eat-on-surface-variant);
    transition: color 0.2s;
}

.dropdown-header button:hover {
    color: var(--eat-primary);
}

/* ── 通知列表 ── */
ul {
    list-style: none;
    margin: 0;
    padding: 0;
    max-height: 360px;
    overflow-y: auto;
}

li {
    display: flex;
    flex-direction: column;
    gap: 2px;
    padding: 0.75rem 1rem;
    cursor: pointer;
    border-bottom: 1px solid var(--eat-outline-variant);
    transition:
        background 0.2s,
        opacity 0.2s;
    opacity: 0.45;
}

li:last-child {
    border-bottom: none;
}

li:hover {
    background: rgba(227, 199, 107, 0.06);
    opacity: 1;
}

/* 未讀：全亮 + 左側金色邊線 */
li.unread {
    opacity: 1;
    border-left: 3px solid var(--eat-primary);
}

/* ── 通知標題行 ── */
.notification-row {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 0.5rem;
}

/* ── 標題截斷 ── */
.title {
    font-family: var(--font-body);
    font-size: 0.875rem;
    color: var(--eat-on-surface);
    line-height: 1.5;
    overflow: hidden;
    white-space: normal;
    text-overflow: ellipsis;
    flex: 1;
    min-width: 0;
    -webkit-line-clamp: 2;
}

/* ── 訊息 ── */
.message {
    font-family: var(--font-body);
    font-size: 0.75rem;
    color: var(--eat-on-surface-variant);
    line-height: 1.4;
    overflow: hidden;
    white-space: normal;
    text-overflow: ellipsis;
    -webkit-line-clamp: 2;
}

/* ── 未讀圓點 ── */
.unread-dot {
    flex-shrink: 0;
    width: 6px;
    height: 6px;
    border-radius: 50%;
    background: var(--eat-primary);
}

/* ── 時間 ── */
.time {
    font-family: var(--font-label);
    font-size: 0.7rem;
    letter-spacing: 0.05em;
    color: var(--eat-on-surface-variant);
}

/* ── 空狀態 ── */
.empty {
    text-align: center;
    padding: 2rem 1rem;
    font-family: var(--font-body);
    font-size: 0.875rem;
    color: var(--eat-on-surface-variant);
}
</style>
