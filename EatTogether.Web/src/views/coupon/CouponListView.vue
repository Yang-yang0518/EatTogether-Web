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

    <div class="container pb-5">

      <!-- 篩選列 -->
      <div class="filter-section">
        <div class="d-flex align-items-center gap-2 flex-wrap">
          <button
            v-for="f in filters"
            :key="f.key"
            class="chip-eat"
            :class="{ active: activeFilter === f.key }"
            @click="activeFilter = f.key"
          >
            <i :class="f.icon" class="me-1"></i>{{ f.label }}
          </button>
        </div>

        <label class="birthday-toggle">
          <input type="checkbox" v-model="birthdayOnly" />
          <span class="toggle-track">
            <span class="toggle-thumb"></span>
          </span>
          <span>🎂 生日專屬</span>
        </label>
      </div>

      <!-- 載入中 -->
      <div v-if="loading" class="d-flex justify-content-center py-5">
        <LoadingSpinner message="載入優惠券中..." />
      </div>

      <!-- 空狀態 -->
      <div v-else-if="!filteredCoupons.length" class="empty-state">
        <i class="bi bi-ticket-perforated empty-icon"></i>
        <p class="eat-body-muted mb-0">目前沒有符合條件的優惠券</p>
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
const pendingMyCoupons = ref(false)   // 未登入點「我的優惠券」時記錄意圖

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

// 登入後：重新抓券；若有待跳轉意圖則導至我的優惠券
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
const coupons      = ref([])
const loading      = ref(true)
const activeFilter = ref('all')
const birthdayOnly = ref(false)
const myUsableCount = ref(0)

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
  margin-bottom: 2.5rem;
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

/* ── 篩選 ──────────────────────────────────────────── */
.filter-section {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 12px;
  margin-bottom: 2rem;
}

/* 自訂 toggle switch */
.birthday-toggle {
  display: flex;
  align-items: center;
  gap: 10px;
  cursor: pointer;
  font-family: var(--font-label);
  font-size: .82rem;
  letter-spacing: .08em;
  color: var(--eat-on-surface-variant);
}
.birthday-toggle input { display: none; }
.toggle-track {
  width: 38px; height: 20px;
  background: var(--eat-surface-high);
  border: 1px solid var(--eat-outline-variant);
  border-radius: 50px;
  position: relative;
  transition: background .25s;
}
.toggle-thumb {
  position: absolute;
  top: 2px; left: 2px;
  width: 14px; height: 14px;
  border-radius: 50%;
  background: var(--eat-outline);
  transition: all .25s;
}
.birthday-toggle input:checked ~ .toggle-track {
  background: rgba(227,199,107,.25);
  border-color: var(--eat-primary);
}
.birthday-toggle input:checked ~ .toggle-track .toggle-thumb {
  transform: translateX(18px);
  background: var(--eat-primary);
}

/* ── 卡片格 ─────────────────────────────────────────── */
.coupon-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(340px, 1fr));
  gap: 16px;
}

/* ── 空狀態 ──────────────────────────────────────────── */
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 16px;
  padding: 5rem 0;
  color: var(--eat-on-surface-variant);
}
.empty-icon {
  font-size: 3rem;
  opacity: .35;
}
</style>
