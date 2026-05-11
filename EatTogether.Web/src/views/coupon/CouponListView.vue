<template>
  <div class="coupon-list-page">

    <!-- Hero -->
    <header class="coupon-hero">
      <p class="eat-label mb-2" style="color:var(--eat-secondary)">
        <i class="bi bi-gift me-2"></i>EXCLUSIVE OFFERS
      </p>
      <h1 class="eat-h1 fst-normal mb-3 eat-display">優惠券專區</h1>
      <div class="hero-sub">
        <div class="hero-line"></div>
        <span class="eat-body-muted">精選折扣優惠，享受義式饗宴更多驚喜</span>
        <div class="hero-line"></div>
      </div>
      <div class="mt-4">
        <Button variant="secondary" @click="goToMyCoupons">
          <i class="bi bi-ticket-perforated me-2"></i>我的優惠券
          <span v-if="authStore.isLoggedIn && myUsableCount > 0" class="ms-1 hero-count">
            {{ myUsableCount }}
          </span>
        </Button>
      </div>
    </header>

    <!-- 主體 -->
    <div class="coupon-body">

      <!-- ── 即將上線區塊 ── -->
      <div v-if="upcomingCoupons.length" class="upcoming-section">
        <div class="upcoming-header">
          <div class="upcoming-title-group">
            <i class="bi bi-alarm upcoming-alarm"></i>
            <span class="upcoming-title">即將登場</span>
            <span class="upcoming-sub">搶先預覽，準時開抢！</span>
          </div>
          <span class="upcoming-badge">{{ upcomingCoupons.length }} 張即將上線</span>
        </div>
        <div class="upcoming-track">
          <div
            v-for="u in upcomingCoupons"
            :key="u.id"
            class="upcoming-card"
          >
            <!-- 鎖定覆蓋層 -->
            <div class="upcoming-lock">
              <i class="bi bi-lock-fill lock-icon"></i>
              <span class="lock-date">{{ formatStartDate(u.startDate) }} 開放</span>
            </div>
            <!-- 卡片主體（模糊處理） -->
            <div class="upcoming-inner">
              <div class="upcoming-discount">{{ u.discountDescription }}</div>
              <div v-if="u.minSpend > 0" class="upcoming-minspend">滿 ${{ u.minSpend }}</div>
              <div class="upcoming-name">{{ u.name }}</div>
              <div class="upcoming-countdown">
                <i class="bi bi-clock me-1"></i>{{ daysUntil(u.startDate) }} 天後開放
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 工具列 -->
      <div class="coupon-toolbar">
        <div class="toolbar-left">
          <i class="bi bi-ticket-perforated toolbar-icon"></i>
          <span class="toolbar-title">優惠精選</span>
          <span v-if="!loading" class="toolbar-count">{{ filteredCoupons.length }} 張</span>
        </div>
        <div class="toolbar-right">
          <div class="chip-group">
            <button
              v-for="f in filters"
              :key="f.key"
              class="chip-btn"
              :class="{ active: activeFilter === f.key }"
              @click="activeFilter = f.key"
            >
              <i :class="f.icon" class="me-1"></i>{{ f.label }}
            </button>
          </div>
          <div class="toolbar-divider"></div>
          <label class="birthday-toggle">
            <input type="checkbox" v-model="birthdayOnly" />
            <span class="toggle-track">
              <span class="toggle-thumb"></span>
            </span>
            <span>🎂 生日專屬</span>
          </label>
        </div>
      </div>

      <!-- 載入中 -->
      <div v-if="loading" class="d-flex justify-content-center py-5">
        <LoadingSpinner message="載入優惠券中..." />
      </div>

      <!-- 空狀態 -->
      <div v-else-if="!filteredCoupons.length" class="empty-state">
        <i class="bi bi-ticket-perforated empty-icon"></i>
        <p class="empty-text">目前沒有符合條件的優惠券</p>
        <p class="empty-sub">試試切換其他篩選條件</p>
      </div>

      <!-- 卡片格 -->
      <div v-else class="coupon-grid">
        <CouponCard
          v-for="c in filteredCoupons"
          :key="c.id"
          :coupon="c"
          @claimed="onClaimed"
        />
      </div>

    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import CouponCard from '@/components/coupon/CouponCard.vue'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'
import Button from '@/components/common/Button.vue'
import apiFetch from '@/utils/apiFetch.js'
import { useAuthStore } from '@/stores/auth.js'

const authStore        = useAuthStore()
const router           = useRouter()
const pendingMyCoupons = ref(false)

async function goToMyCoupons() {
  if (!authStore.isLoggedIn) {
    pendingMyCoupons.value = true
    const modalEl = document.querySelector('#authModal')
    if (modalEl) {
      const { Modal } = await import('bootstrap')
      Modal.getOrCreateInstance(modalEl).show()
    }
    return
  }
  router.push({ name: 'MyCoupons' })
}

watch(() => authStore.isLoggedIn, (loggedIn) => {
  if (loggedIn) {
    fetchCoupons()
    fetchMyUsableCount()
    if (pendingMyCoupons.value) {
      pendingMyCoupons.value = false
      router.push({ name: 'MyCoupons' })
    }
  }
})

const coupons         = ref([])
const upcomingCoupons = ref([])
const loading         = ref(true)
const activeFilter    = ref('all')
const birthdayOnly    = ref(false)
const myUsableCount   = ref(0)

const filters = [
  { key: 'all', label: '全部',    icon: 'bi bi-grid' },
  { key: '0',   label: '折金額',  icon: 'bi bi-cash' },
  { key: '1',   label: '折百分比', icon: 'bi bi-percent' },
]

const filteredCoupons = computed(() => {
  let list = coupons.value
  if (activeFilter.value !== 'all')
    list = list.filter(c => String(c.discountType) === activeFilter.value)
  if (birthdayOnly.value)
    list = list.filter(c => c.code?.startsWith('BDAY'))
  return list
})

function onClaimed(couponId) {
  const idx = coupons.value.findIndex(c => c.id === couponId)
  if (idx !== -1) coupons.value[idx] = { ...coupons.value[idx], isClaimed: true }
}

async function fetchCoupons() {
  loading.value = true
  try {
    const res = await apiFetch('/Coupons')
    if (res.ok) coupons.value = await res.json()
  } finally {
    loading.value = false
  }
}

async function fetchUpcoming() {
  try {
    const res = await apiFetch('/Coupons/Upcoming')
    if (res.ok) upcomingCoupons.value = await res.json()
  } catch { /* ignore */ }
}

function daysUntil(dt) {
  return Math.ceil((new Date(dt) - Date.now()) / 86400000)
}

function formatStartDate(dt) {
  return new Date(dt).toLocaleDateString('zh-TW', { month: 'numeric', day: 'numeric' })
}

async function fetchMyUsableCount() {
  if (!authStore.isLoggedIn) return
  try {
    const res = await apiFetch('/Coupons/My')
    if (res.ok) {
      const list = await res.json()
      const now = new Date()
      myUsableCount.value = list.filter(mc =>
        !mc.isUsed && (!mc.endDate || new Date(mc.endDate) >= now)
      ).length
    }
  } catch { /* ignore */ }
}

onMounted(() => {
  fetchCoupons()
  fetchMyUsableCount()
  fetchUpcoming()
})
</script>

<style scoped>
.coupon-list-page {
  min-height: 100vh;
  background: var(--eat-bg);
}

/* ── Hero ─────────────────────────────────────────── */
.coupon-hero {
  padding: 5rem 2rem 4rem;
  text-align: center;
  background-color: var(--eat-surface-container);
  border-bottom: 1px solid var(--eat-outline-variant);
}
.hero-sub {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 1.25rem;
  opacity: 0.75;
}
.hero-line {
  flex: 0 0 3rem;
  height: 1px;
  background: var(--eat-secondary);
  opacity: 0.3;
}
.hero-count {
  background: rgba(227,199,107,.2);
  color: var(--eat-primary);
  font-size: .72rem;
  padding: .1rem .5rem;
  border-radius: 50px;
  border: 1px solid rgba(227,199,107,.3);
}

/* ── 主體 ─────────────────────────────────────────── */
.coupon-body {
  max-width: 1160px;
  margin: 0 auto;
  padding: 3rem 2rem 6rem;
}

/* ── 即將上線 ─────────────────────────────────────── */
.upcoming-section {
  margin-bottom: 2.5rem;
  background: var(--eat-surface-container);
  border: 1px solid var(--eat-outline-variant);
  border-radius: var(--eat-radius-lg);
  overflow: hidden;
}

.upcoming-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: .85rem 1.5rem;
  border-bottom: 1px solid var(--eat-outline-variant);
  background: linear-gradient(90deg, rgba(227,199,107,.06) 0%, transparent 60%);
}
.upcoming-title-group {
  display: flex;
  align-items: center;
  gap: 10px;
}
.upcoming-alarm {
  color: var(--eat-primary);
  font-size: 1rem;
  animation: ring .8s ease infinite alternate;
}
@keyframes ring {
  from { transform: rotate(-12deg); }
  to   { transform: rotate(12deg);  }
}
.upcoming-title {
  font-family: var(--font-headline);
  font-size: .9rem;
  font-weight: 600;
  color: var(--eat-on-surface);
  letter-spacing: .04em;
}
.upcoming-sub {
  font-family: var(--font-body);
  font-size: .75rem;
  color: var(--eat-on-surface-variant);
  opacity: .6;
  font-style: italic;
}
.upcoming-badge {
  font-family: var(--font-label);
  font-size: .7rem;
  letter-spacing: .1em;
  color: var(--eat-secondary);
  background: rgba(227,199,107,.1);
  border: 1px solid rgba(227,199,107,.2);
  border-radius: 50px;
  padding: .18rem .75rem;
}

/* 橫向捲動列 */
.upcoming-track {
  display: flex;
  gap: 14px;
  padding: 1.25rem 1.5rem;
  overflow-x: auto;
  scroll-snap-type: x mandatory;
  scrollbar-width: none;
}
.upcoming-track::-webkit-scrollbar { display: none; }

/* 單張預覽卡 */
.upcoming-card {
  position: relative;
  flex: 0 0 180px;
  height: 140px;
  border-radius: var(--eat-radius-md);
  border: 1px solid var(--eat-outline-variant);
  overflow: hidden;
  scroll-snap-align: start;
  background: linear-gradient(150deg, #2a1308 0%, #1a0d07 100%);
  transition: border-color .2s;
}
.upcoming-card:hover {
  border-color: rgba(227,199,107,.3);
}

/* 模糊底層 */
.upcoming-inner {
  position: absolute;
  inset: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 5px;
  padding: 1rem;
  text-align: center;
  filter: blur(4px);
  opacity: .4;
  user-select: none;
}
.upcoming-discount {
  font-family: var(--font-headline);
  font-size: 1.35rem;
  font-weight: 700;
  color: var(--eat-primary);
}
.upcoming-minspend {
  font-size: .62rem;
  color: var(--eat-on-surface-variant);
  font-family: var(--font-label);
}
.upcoming-name {
  font-size: .75rem;
  color: var(--eat-on-surface);
  font-family: var(--font-label);
}
.upcoming-countdown {
  font-size: .62rem;
  color: var(--eat-secondary);
}

/* 鎖定覆蓋 */
.upcoming-lock {
  position: absolute;
  inset: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 8px;
  background: rgba(15, 8, 4, .55);
  backdrop-filter: blur(1px);
  z-index: 2;
}
.lock-icon {
  font-size: 1.4rem;
  color: var(--eat-primary);
  opacity: .85;
}
.lock-date {
  font-family: var(--font-label);
  font-size: .72rem;
  letter-spacing: .1em;
  color: var(--eat-primary);
  background: rgba(227,199,107,.12);
  border: 1px solid rgba(227,199,107,.25);
  border-radius: 50px;
  padding: .2rem .75rem;
}

/* ── 工具列 ──────────────────────────────────────── */
.coupon-toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 14px;
  padding: 1rem 1.5rem;
  background: var(--eat-surface-container);
  border: 1px solid var(--eat-outline-variant);
  border-radius: var(--eat-radius-lg);
  margin-bottom: 2rem;
}

.toolbar-left {
  display: flex;
  align-items: center;
  gap: 10px;
}
.toolbar-icon {
  color: var(--eat-secondary);
  font-size: 1rem;
}
.toolbar-title {
  font-family: var(--font-headline);
  font-size: .92rem;
  font-weight: 600;
  color: var(--eat-on-surface);
  letter-spacing: .04em;
}
.toolbar-count {
  font-family: var(--font-label);
  font-size: .72rem;
  letter-spacing: .1em;
  color: var(--eat-primary);
  background: rgba(227,199,107,.12);
  border: 1px solid rgba(227,199,107,.25);
  border-radius: 50px;
  padding: .15rem .65rem;
}

.toolbar-right {
  display: flex;
  align-items: center;
  gap: 14px;
  flex-wrap: wrap;
}

.chip-group {
  display: flex;
  gap: 6px;
}
.chip-btn {
  display: flex;
  align-items: center;
  padding: .35rem .9rem;
  border-radius: 50px;
  border: 1px solid var(--eat-outline-variant);
  background: transparent;
  color: var(--eat-on-surface-variant);
  font-family: var(--font-label);
  font-size: .78rem;
  letter-spacing: .06em;
  cursor: pointer;
  transition: all .2s;
}
.chip-btn:hover {
  border-color: rgba(227,199,107,.4);
  color: var(--eat-on-surface);
}
.chip-btn.active {
  background: rgba(227,199,107,.15);
  border-color: var(--eat-primary);
  color: var(--eat-primary);
}

.toolbar-divider {
  width: 1px;
  height: 20px;
  background: var(--eat-outline-variant);
  opacity: .6;
}

/* 自訂 toggle switch */
.birthday-toggle {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  font-family: var(--font-label);
  font-size: .78rem;
  letter-spacing: .06em;
  color: var(--eat-on-surface-variant);
  user-select: none;
}
.birthday-toggle input { display: none; }
.toggle-track {
  width: 36px; height: 19px;
  background: var(--eat-surface-high);
  border: 1px solid var(--eat-outline-variant);
  border-radius: 50px;
  position: relative;
  transition: background .25s;
  flex-shrink: 0;
}
.toggle-thumb {
  position: absolute;
  top: 2px; left: 2px;
  width: 13px; height: 13px;
  border-radius: 50%;
  background: var(--eat-outline);
  transition: all .25s;
}
.birthday-toggle input:checked ~ .toggle-track {
  background: rgba(227,199,107,.2);
  border-color: var(--eat-primary);
}
.birthday-toggle input:checked ~ .toggle-track .toggle-thumb {
  transform: translateX(17px);
  background: var(--eat-primary);
}

/* ── 卡片格 ─────────────────────────────────────────── */
.coupon-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(360px, 1fr));
  gap: 18px;
}

/* ── 空狀態 ──────────────────────────────────────────── */
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 10px;
  padding: 6rem 0;
  text-align: center;
}
.empty-icon {
  font-size: 2.8rem;
  color: var(--eat-secondary);
  opacity: .25;
  margin-bottom: .5rem;
}
.empty-text {
  font-family: var(--font-headline);
  font-size: .95rem;
  color: var(--eat-on-surface);
  opacity: .5;
  margin: 0;
}
.empty-sub {
  font-family: var(--font-body);
  font-size: .8rem;
  color: var(--eat-on-surface-variant);
  opacity: .4;
  font-style: italic;
  margin: 0;
}

/* ── RWD ──────────────────────────────────────────── */
@media (max-width: 767px) {
  .coupon-body { padding: 2rem 1.25rem 4rem; }
  .coupon-toolbar { flex-direction: column; align-items: flex-start; }
  .toolbar-right { width: 100%; justify-content: space-between; }
  .coupon-grid { grid-template-columns: 1fr; }
}
</style>
