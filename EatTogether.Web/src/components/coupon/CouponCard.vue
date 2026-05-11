<template>
  <div class="coupon-card" :class="{ claimed: coupon.isClaimed }">

    <!-- 左側主視覺 -->
    <div class="coupon-left">
      <!-- 類型標籤 -->
      <div class="type-tag">
        <i :class="coupon.discountType === 0 ? 'bi bi-cash' : 'bi bi-percent'" class="me-1"></i>
        {{ coupon.discountType === 0 ? '折金額' : '折百分比' }}
      </div>
      <!-- 折扣值 -->
      <div class="discount-value">{{ coupon.discountDescription }}</div>
      <div v-if="coupon.minSpend > 0" class="min-spend">消費滿 ${{ coupon.minSpend }}</div>
      <!-- 序號 -->
      <div class="coupon-code-badge">{{ coupon.code }}</div>
    </div>

    <!-- 鋸齒切邊 -->
    <div class="perforation">
      <div class="notch top"></div>
      <div class="dash-line"></div>
      <div class="notch bottom"></div>
    </div>

    <!-- 右側資訊 -->
    <div class="coupon-right">
      <div class="coupon-name">{{ coupon.name }}</div>

      <div class="coupon-meta">
        <span v-if="coupon.endDate" :class="{ 'expiry-warn': isExpiringSoon(coupon.endDate) }" class="meta-item">
          <i class="bi bi-clock me-1"></i>{{ formatExpire(coupon.endDate) }}
        </span>
        <span v-if="coupon.limitCount" class="meta-item remain">
          <i class="bi bi-people me-1"></i>剩 {{ coupon.limitCount - coupon.receivedCount }} 張
        </span>
      </div>

      <!-- 底部：已領取標記 or 領取按鈕 -->
      <div class="card-footer">
        <span v-if="coupon.isClaimed" class="claimed-badge">
          <i class="bi bi-check-circle-fill me-1"></i>已領取
        </span>
        <button
          v-else
          class="claim-btn"
          :class="{ 'claim-btn--loading': claiming }"
          :disabled="claiming"
          @click="handleClaim"
        >
          <span v-if="claiming" class="spinner-border spinner-border-sm me-1" style="width:.8rem;height:.8rem;border-width:2px"></span>
          <i v-else class="bi bi-bag-plus me-1"></i>
          <span>{{ isLoggedIn ? '立即領取' : '登入後領取' }}</span>
        </button>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useAuthStore } from '@/stores/auth.js'
import apiFetch from '@/utils/apiFetch.js'
import { useToast } from '@/composables/useToast.js'

const props = defineProps({
  coupon: { type: Object, required: true }
})
const emit = defineEmits(['claimed'])

const authStore  = useAuthStore()
const { show }   = useToast()
const isLoggedIn = computed(() => authStore.isLoggedIn)
const claiming   = ref(false)

async function handleClaim() {
  if (!isLoggedIn.value) {
    const modalEl = document.querySelector('#authModal')
    if (modalEl) {
      const { Modal } = await import('bootstrap')
      Modal.getOrCreateInstance(modalEl).show()
    }
    return
  }

  claiming.value = true
  try {
    const res = await apiFetch(`/Coupons/${props.coupon.id}/Claim`, { method: 'POST' })
    if (res.ok) {
      show('領取成功！已加入您的優惠券', 'success')
      emit('claimed', props.coupon.id)
    } else {
      const err = await res.json().catch(() => ({}))
      show(err.message || '領取失敗', 'error')
    }
  } catch {
    show('網路錯誤，請稍後再試', 'error')
  } finally {
    claiming.value = false
  }
}

function isExpiringSoon(dt) {
  return Math.ceil((new Date(dt) - Date.now()) / 86400000) <= 3
}

function formatExpire(dt) {
  const diff = Math.ceil((new Date(dt) - Date.now()) / 86400000)
  if (diff <= 0) return '已過期'
  if (diff <= 3) return `僅剩 ${diff} 天`
  return new Date(dt).toLocaleDateString('zh-TW')
}
</script>

<style scoped>
/* ── 外框 ──────────────────────────────────────────── */
.coupon-card {
  display: flex;
  align-items: stretch;
  background: var(--eat-surface-container);
  border: 1px solid var(--eat-outline-variant);
  border-radius: var(--eat-radius-lg);
  overflow: hidden;
  transition: transform .22s ease, box-shadow .22s ease, border-color .22s;
  position: relative;
}
.coupon-card::before {
  content: '';
  position: absolute;
  inset: 0;
  border-radius: var(--eat-radius-lg);
  background: linear-gradient(135deg, rgba(227,199,107,.05) 0%, transparent 55%);
  pointer-events: none;
}
.coupon-card:not(.claimed):hover {
  transform: translateY(-4px);
  box-shadow: 0 16px 40px rgba(0,0,0,.5), 0 0 0 1px rgba(227,199,107,.25);
  border-color: rgba(227,199,107,.4);
}
.coupon-card.claimed {
  opacity: .45;
  filter: grayscale(.4);
}

/* ── 左側色塊 ─────────────────────────────────────── */
.coupon-left {
  width: 140px;
  flex-shrink: 0;
  background: linear-gradient(160deg, #3d1a08 0%, #261510 55%, #1a0d07 100%);
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 7px;
  padding: 1.4rem 0.8rem;
  text-align: center;
  position: relative;
}
/* 光暈效果 */
.coupon-left::after {
  content: '';
  position: absolute;
  inset: 0;
  background: radial-gradient(ellipse at 50% 35%, rgba(227,199,107,.15) 0%, transparent 68%);
  pointer-events: none;
}

.type-tag {
  font-family: var(--font-label);
  font-size: .58rem;
  letter-spacing: .12em;
  text-transform: uppercase;
  color: rgba(227,199,107,.55);
  background: rgba(227,199,107,.08);
  border: 1px solid rgba(227,199,107,.18);
  border-radius: 2px;
  padding: .12rem .5rem;
  position: relative;
  z-index: 1;
}
.discount-value {
  font-family: var(--font-headline);
  font-size: 1.5rem;
  font-weight: 700;
  color: var(--eat-primary);
  line-height: 1.15;
  position: relative;
  z-index: 1;
}
.min-spend {
  font-family: var(--font-label);
  font-size: .62rem;
  letter-spacing: .06em;
  color: var(--eat-on-surface-variant);
  position: relative;
  z-index: 1;
}
.coupon-code-badge {
  font-family: var(--font-label);
  font-size: .58rem;
  letter-spacing: .14em;
  color: rgba(227,199,107,.45);
  position: relative;
  z-index: 1;
  margin-top: 2px;
}

/* ── 鋸齒分隔 ─────────────────────────────────────── */
.perforation {
  width: 16px;
  flex-shrink: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  background: var(--eat-surface-container);
  position: relative;
}
.notch {
  width: 16px;
  height: 8px;
  background: var(--eat-bg);
  flex-shrink: 0;
}
.notch.top {
  border-radius: 0 0 8px 8px;
  border-bottom: 1px solid var(--eat-outline-variant);
  border-left: 1px solid var(--eat-outline-variant);
  border-right: 1px solid var(--eat-outline-variant);
}
.notch.bottom {
  border-radius: 8px 8px 0 0;
  border-top: 1px solid var(--eat-outline-variant);
  border-left: 1px solid var(--eat-outline-variant);
  border-right: 1px solid var(--eat-outline-variant);
}
.dash-line {
  flex: 1;
  width: 1px;
  background: repeating-linear-gradient(
    to bottom,
    var(--eat-outline-variant),
    var(--eat-outline-variant) 4px,
    transparent 4px,
    transparent 8px
  );
  margin: 0 auto;
}

/* ── 右側資訊 ─────────────────────────────────────── */
.coupon-right {
  flex: 1;
  padding: 1.2rem 1.4rem;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  gap: 10px;
  min-height: 110px;
}
.coupon-name {
  font-family: var(--font-headline);
  font-size: 1rem;
  color: var(--eat-on-surface);
  font-weight: 600;
  line-height: 1.45;
}
.coupon-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
}
.meta-item {
  display: flex;
  align-items: center;
  font-size: .73rem;
  color: var(--eat-on-surface-variant);
}
.expiry-warn { color: #e09a50; }
.remain { color: var(--eat-secondary); }

/* ── 底部 ─────────────────────────────────────────── */
.card-footer {
  display: flex;
  align-items: center;
}

/* 已領取標記 */
.claimed-badge {
  display: inline-flex;
  align-items: center;
  font-family: var(--font-label);
  font-size: .75rem;
  letter-spacing: .08em;
  color: var(--eat-on-surface-variant);
  opacity: .6;
}

/* 領取按鈕 */
.claim-btn {
  display: inline-flex;
  align-items: center;
  padding: .42rem 1.2rem;
  border-radius: var(--eat-radius-sm);
  font-family: var(--font-label);
  font-size: .78rem;
  letter-spacing: .1em;
  cursor: pointer;
  border: 1px solid var(--eat-primary);
  color: var(--eat-primary);
  background: transparent;
  transition: all .2s;
}
.claim-btn:hover:not(:disabled) {
  background: var(--eat-primary);
  color: var(--eat-on-primary);
  box-shadow: 0 4px 18px rgba(227,199,107,.28);
}
.claim-btn:disabled { cursor: not-allowed; opacity: .7; }
</style>
