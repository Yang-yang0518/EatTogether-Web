<template>
    <nav class="navbar navbar-expand-lg navbar-eat px-lg-3 px-0">
        <div class="container-fluid">
            <!-- Brand Logo -->
            <RouterLink class="navbar-brand py-0" to="/">
                <img
                    src="@/assets/images/logo.svg"
                    alt="義起吃 Eat Together"
                    height="50"
                    class="d-inline-block align-middle"
                />
            </RouterLink>

            <!-- Mobile Toggler -->
            <div class="d-flex align-items-center gap-2 ms-auto d-lg-none">
                <BellNotification v-if="authStore.isLoggedIn" />

                <button
                    class="navbar-toggler"
                    type="button"
                    data-bs-toggle="collapse"
                    data-bs-target="#navbarMain"
                    aria-controls="navbarMain"
                    aria-expanded="false"
                    aria-label="Toggle navigation"
                >
                    <span class="navbar-toggler-icon"></span>
                </button>
            </div>

            <!-- Nav Links -->
            <div class="collapse navbar-collapse" id="navbarMain" ref="navbarCollapse">
                <div
                    class="w-100 d-flex justify-content-between flex-column-reverse flex-lg-row px-2 px-lg-0"
                >
                    <ul
                        class="navbar-nav w-100 gap-2 gap-lg-4 align-items-stretch align-items-lg-center justify-content-center"
                    >
                        <template v-for="link in navLinks" :key="link.to ?? link.label">
                            <!-- Dropdown -->
                            <li
                                v-if="link.children"
                                class="nav-item dropdown-wrap"
                                :class="{ 'is-open': openDropdown === link.label }"
                                @mouseenter="isHoverDevice && (openDropdown = link.label)"
                                @mouseleave="isHoverDevice && (openDropdown = null)"
                            >
                                <button
                                    class="nav-link nav-dropdown-trigger"
                                    :class="{ active: link.children.some((c) => c.to && isActive(typeof c.to === 'string' ? c.to : c.to.path)) }"
                                    @click.stop="toggleDropdown(link.label)"
                                >
                                    {{ link.label }}
                                    <i class="bi bi-chevron-down dropdown-chevron"></i>
                                </button>
                                <Transition name="dropdown">
                                    <div
                                        v-if="openDropdown === link.label"
                                        class="dropdown-menu-eat"
                                    >
                                        <RouterLink
                                            v-for="child in link.children"
                                            :key="child.label"
                                            class="dropdown-item-eat"
                                            :to="child.to"
                                            :class="{ active: child.to && isActive(typeof child.to === 'string' ? child.to : child.to.path) }"
                                            @click="handleLinkClick"
                                            >{{ child.label }}</RouterLink
                                        >
                                    </div>
                                </Transition>
                            </li>
                            <!-- Normal link -->
                            <li v-else class="nav-item">
                                <RouterLink
                                    class="nav-link"
                                    :to="link.to"
                                    :class="{ active: isActive(link.to) }"
                                    @click="handleLinkClick"
                                >
                                    {{ link.label }}
                                </RouterLink>
                            </li>
                        </template>
                    </ul>

                    <!-- CTA / 使用者區塊 -->
                    <div class="d-flex align-items-center justify-content-center my-3 my-lg-0">
                        <!-- 未登入 -->
                        <Button
                            v-if="!authStore.isLoggedIn"
                            class="btn-eat-sm w-100 w-lg-auto"
                            variant="primary"
                            @click="openAuthModal"
                            aria-label="開啟登入或註冊視窗"
                        >
                            登入｜註冊
                        </Button>

                        <!-- 已登入：小鈴鐺 -->
                        <BellNotification
                            v-if="authStore.isLoggedIn"
                            class="d-none d-lg-inline-block"
                        />

                        <!-- 已登入：頭像 + Dropdown --><!-- v-else更改條件式 -->
                        <div
                            v-if="authStore.isLoggedIn"
                            class="w-100 w-lg-auto nav-item dropdown-wrap d-flex flex-column align-items-center justify-content-center"
                            :class="{ 'is-open': openDropdown === 'user' }"
                            @mouseenter="isHoverDevice && (openDropdown = 'user')"
                            @mouseleave="isHoverDevice && (openDropdown = null)"
                        >
                            <button
                                class="nav-link nav-dropdown-trigger d-flex justify-content-center mb-2 mb-lg-0 py-0"
                                type="button"
                                @click.stop="toggleDropdown('user')"
                                aria-label="開啟使用者選單"
                            >
                                <AvatarInitial
                                    class="avatar-wrapper"
                                    :avatarFileName="authStore.member.avatarFileName"
                                    :googleAvatarUrl="authStore.member.googleAvatarUrl"
                                    :name="authStore.member.name"
                                    size="36px"
                                    interactive
                                />
                            </button>
                            <Transition name="dropdown">
                                <div
                                    v-if="openDropdown === 'user'"
                                    class="dropdown-menu-eat dropdown-menu-eat--right"
                                >
                                    <RouterLink
                                        to="/member"
                                        class="dropdown-item-eat"
                                        @click="handleLinkClick"
                                        >會員中心</RouterLink
                                    >
                                    <button
                                        class="dropdown-item-eat dropdown-item-eat--btn"
                                        type="button"
                                        @click="authStore.logout()"
                                    >
                                        登出
                                    </button>
                                </div>
                            </Transition>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </nav>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { RouterLink } from 'vue-router'
import Button from '@/components/common/Button.vue'
import AvatarInitial from '@/components/member/AvatarInitial.vue'
import { Modal, Collapse } from 'bootstrap'
import { useAuthStore } from '@/stores/auth.js'
import BellNotification from '@/components/common/notification/BellNotification.vue'

const authStore = useAuthStore()

const route = useRoute()
const openDropdown = ref(null)
const navbarCollapse = ref(null)
const isHoverDevice = typeof window !== 'undefined' && window.matchMedia('(hover: hover)').matches

const navLinks = [
    { label: '最新消息', to: '/news' },
    {
        label: '菜單分類',
        children: [
            { label: '本季限定', to: '/limited' },
            { label: '精選菜單', to: '/menu' },
            { label: '精選套餐', to: '/setmeal' },
        ],
    },
    { label: '優惠券', to: '/coupons' },
    { label: '訂位', to: '/reservation' },
    { label: '候位', to: '/walkin' },
    {
        label: '享用美味',
        children: [
            { label: '外帶點餐', to: '/takeout' },
            { label: '當日訂單查詢', to: '/order-lookup' },
        ],
    },
]

function isActive(path) {
    if (path === '/') return route.path === '/'
    return route.path.startsWith(path)
}

function toggleDropdown(label) {
    // hover 裝置由 mouseenter/mouseleave 控制，click 不介入
    if (isHoverDevice) return
    openDropdown.value = openDropdown.value === label ? null : label
}

function openAuthModal() {
    const modalEl = document.querySelector('#authModal')
    const modal = new Modal(modalEl)
    modal.show()
}

function handleLinkClick() {
    // 1. 關閉 Vue 控制的子選單
    openDropdown.value = null

    // 2. 關閉 Bootstrap 控制的手機版選單
    if (navbarCollapse.value) {
        // 取得或建立 Bootstrap Collapse 實例
        const bsCollapse =
            Collapse.getInstance(navbarCollapse.value) ||
            new Collapse(navbarCollapse.value, { toggle: false })
        bsCollapse.hide()
    }
}

// 點擊外部關閉
if (typeof window !== 'undefined') {
    window.addEventListener('click', (e) => {
        if (!e.target.closest('.dropdown-wrap')) openDropdown.value = null
    })
}

onMounted(() => {
    const navbarEl = document.querySelector('#navbarMain')

    // 當選單「開始展開」時
    navbarEl.addEventListener('show.bs.collapse', () => {
        document.body.style.overflow = 'hidden'
    })

    // 當選單「開始收合」時
    navbarEl.addEventListener('hide.bs.collapse', () => {
        document.body.style.overflow = ''
    })
})
</script>

<style scoped>
.navbar-eat {
    background: rgba(41, 24, 17, 0.72);
    backdrop-filter: blur(20px);
    -webkit-backdrop-filter: blur(20px);
    border-bottom: 1px solid rgba(180, 120, 30, 0.12);
    /* 固定在頂部，讓 Hero 可以透出 navbar */
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    z-index: 1030;
}

.navbar-eat .nav-link {
    display: block;
    font-family: var(--font-body);
    text-align: center;
    font-size: 1rem;
    letter-spacing: 0.06em;
    color: rgba(226, 210, 185, 0.85);
    transition: color 0.3s ease;
    padding: 0.5rem 1rem;
    position: relative;
    text-transform: none;
    text-decoration: none;
    width: 100%;
}
.navbar-eat .nav-link:hover {
    color: rgba(254, 225, 130, 0.9);
}
.navbar-eat .nav-link.active {
    color: var(--eat-primary);
    font-weight: 700;
}
/* active 底線 */
.navbar-eat .nav-link.active::after {
    content: '';
    display: block;
    height: 2px;
    background: var(--eat-primary);
    position: absolute;
    bottom: 2px;
    left: 0;
    right: 0;
}

@media (max-width: 992px) {
    .navbar-eat .nav-link.active::after {
        display: none;
    }
}

/* ── Dropdown ── */
.dropdown-wrap {
    position: relative;
}

/* 為了防止選單與按鈕之間有空隙導致 hover 消失 */
.dropdown-wrap::after {
    content: '';
    position: absolute;
    top: 100%;
    left: 0;
    width: 100%;
    height: 1rem;
    display: block;
}

@media (max-width: 992px) {
    .dropdown-wrap::after {
        display: none;
    }
}

.nav-dropdown-trigger {
    background: none;
    border: none;
    display: flex;
    align-items: center;
    gap: 0.35rem;
    cursor: pointer;
}
.dropdown-chevron {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    transition: transform 0.25s ease;
    opacity: 0.6;
    font-size: 0.8rem;
    line-height: 1;
}
.is-open .dropdown-chevron {
    transform: rotate(180deg);
}
.dropdown-menu-eat {
    position: absolute;
    top: calc(100% + 0.75rem);
    left: 50%;
    transform: translateX(-50%);
    background: rgba(30, 16, 10, 0.96);
    backdrop-filter: blur(20px);
    border: 1px solid rgba(180, 120, 30, 0.2);
    border-radius: 0.125rem;
    padding: 0.4rem;
    min-width: 130px;
    display: flex;
    flex-direction: column;
    gap: 0.1rem;
    box-shadow: 0 16px 40px rgba(0, 0, 0, 0.5);
    z-index: 100;
}

@media (max-width: 992px) {
    .dropdown-menu-eat {
        position: static;
        transform: none;
        left: auto;
        top: auto;

        /* 讓寬度撐滿父層，並調整內距 */
        width: 100%;
        min-width: unset;
        /* margin-top: 0.5rem; */
        padding: 0.5rem; /* 增加選單內框距離 */

        box-shadow: none;
        border-radius: 0.125rem;
        border: none;
        background: rgba(255, 255, 255, 0.1); /* 給個微弱背影深淺色差，區隔感會更好 */
    }
}

.dropdown-item-eat {
    display: block;
    font-family: var(--font-body);
    text-align: center;
    font-size: 0.95rem;
    letter-spacing: 0.06em;
    color: rgba(226, 210, 185, 0.85);
    text-decoration: none;
    padding: 0.8rem 1.5rem;
    border-radius: 8px;
    transition:
        background 0.2s,
        color 0.2s;
    white-space: nowrap;
}
.dropdown-item-eat:hover {
    background: rgba(227, 199, 107, 0.08);
    color: rgba(254, 225, 130, 0.9);
}
.dropdown-item-eat.active {
    color: var(--eat-primary);
    font-weight: 700;
}

/* Dropdown 動畫 */
.dropdown-enter-active {
    transition:
        opacity 0.2s ease,
        transform 0.2s cubic-bezier(0.22, 1, 0.36, 1);
}
.dropdown-leave-active {
    transition:
        opacity 0.15s ease,
        transform 0.15s ease;
}
.dropdown-enter-from {
    opacity: 0;
    transform: translateX(-50%) translateY(-6px);
}
.dropdown-leave-to {
    opacity: 0;
    transform: translateX(-50%) translateY(-4px);
}

@media (max-width: 992px) {
    .dropdown-enter-from {
        opacity: 0;
        transform: translateY(-6px);
    }
    .dropdown-leave-to {
        opacity: 0;
        transform: translateY(-4px);
    }
}

/* Mobile toggler */
/* 關掉特定元素 */
.navbar-eat .navbar-toggler {
    padding: 0.5rem;
    margin-left: 0.25rem;
    border: none;
}

.navbar-eat .navbar-toggler:focus {
    box-shadow: none;
    outline: none;
}

.navbar-eat .navbar-toggler-icon {
    width: 1.5rem;
    height: 1.5rem;
    background-image: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 30 30'%3e%3cpath stroke='rgba(249,221,211,0.8)' stroke-linecap='round' stroke-miterlimit='10' stroke-width='2' d='M4 7h22M4 15h22M4 23h22'/%3e%3c/svg%3e");
}

@media (max-width: 992px) {
    .navbar-collapse {
        /* 設定最大高度，避免選單太長滑不到底 */
        max-height: 80vh;
        overflow-y: auto; /* 讓選單內部可以捲動 */
        padding-bottom: 2rem;

        /* 順滑捲動體驗 */
        -webkit-overflow-scrolling: touch;
    }
}

/* Avatar（border/hover/cursor 已移交 AvatarInitial interactive prop 控制）*/
@media (max-width: 992px) {
    .nav-link .avatar-wrapper {
        width: 48px !important;
        height: 48px !important;
    }
}

/* dropdown-item-eat button 重設（無背景、無邊框的 <button> 版本）*/
.dropdown-item-eat--btn {
    width: 100%;
    background: none;
    border: none;
    text-align: center;
    cursor: pointer;
}

/* 頭像選單：靠右對齊，不使用水平置中 */
.dropdown-menu-eat--right {
    left: auto;
    right: 0;
    transform: none;
}

/* 頭像選單動畫：覆寫 translateX(-50%) 避免跑版 */
.dropdown-menu-eat--right.dropdown-enter-from,
.dropdown-menu-eat--right.dropdown-leave-to {
    transform: translateY(-6px);
}
</style>
