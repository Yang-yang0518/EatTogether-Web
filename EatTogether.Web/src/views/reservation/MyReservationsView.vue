<template>
  <div class="d-flex flex-column gap-4">

    <div>
      <h2 class="eat-h3 fst-normal mb-1">我的訂位</h2>
      <p class="eat-body-muted mb-0">管理您的訂位紀錄</p>
    </div>

    <!-- 載入中 -->
    <LoadingSpinner v-if="loading" message="載入訂位資料中..." />

    <template v-else>
      <!-- 即將到來 -->
      <section>
        <h3 class="eat-label mb-3">即將到來</h3>
        <div v-if="!upcoming.length" class="card-eat p-4 text-center">
          <p class="eat-body-muted mb-3">目前沒有即將到來的訂位</p>
          <Button variant="primary" :to="{ name: 'Reservation' }">立即訂位</Button>
        </div>
        <div v-else class="d-flex flex-column gap-3">
          <div v-for="r in upcoming" :key="r.id" class="card-eat p-4">
            <div class="d-flex justify-content-between align-items-start mb-2">
              <div>
                <span class="eat-label me-2">{{ r.bookingNumber }}</span>
                <span :class="statusClass(r.status)" class="badge-status">{{ r.statusText }}</span>
              </div>
              <div v-if="r.status === 0" class="d-flex flex-column align-items-end gap-1">
                <Button
                  variant="danger"
                  class="btn-eat-sm"
                  :disabled="!canCancel(r)"
                  :loading="cancelling && cancelTarget?.id === r.id"
                  @click="canCancel(r) && openCancel(r)"
                >取消訂位</Button>
                <span v-if="!canCancel(r)" class="cancel-hint">
                  <i class="bi bi-clock me-1"></i>訂位前 1 小時內請來電取消
                </span>
              </div>
            </div>
            <div class="feather-divider my-2" style="margin:0.75rem 0"></div>
            <div class="row g-2 eat-body">
              <div class="col-6 col-md-3">
                <span class="eat-label d-block mb-1">日期</span>
                {{ formatDate(r.reservationDate) }}
              </div>
              <div class="col-6 col-md-3">
                <span class="eat-label d-block mb-1">時間</span>
                {{ formatTime(r.reservationDate) }}
              </div>
              <div class="col-6 col-md-3">
                <span class="eat-label d-block mb-1">人數</span>
                大人 {{ r.adultsCount }}・小孩 {{ r.childrenCount }}
              </div>
              <div class="col-6 col-md-3">
                <span class="eat-label d-block mb-1">姓名</span>
                {{ r.name }}
              </div>
            </div>
            <p v-if="r.remark" class="eat-body-muted mb-0 mt-2">
              <i class="bi bi-chat-left-text me-1"></i>{{ r.remark }}
            </p>
          </div>
        </div>
      </section>

      <div class="feather-divider"></div>

      <!-- 歷史紀錄 -->
      <section>
        <h3 class="eat-label mb-3">歷史紀錄</h3>
        <div v-if="!history.length" class="card-eat p-4 text-center">
          <p class="eat-body-muted mb-0">尚無歷史訂位紀錄</p>
        </div>
        <div v-else class="d-flex flex-column gap-3">
          <div v-for="r in history" :key="r.id" class="card-eat p-4" style="opacity:.7">
            <div class="d-flex justify-content-between align-items-center mb-2">
              <span class="eat-label">{{ r.bookingNumber }}</span>
              <span :class="statusClass(r.status)" class="badge-status">{{ r.statusText }}</span>
            </div>
            <div class="feather-divider my-2" style="margin:0.75rem 0"></div>
            <div class="row g-2 eat-body">
              <div class="col-6 col-md-3">
                <span class="eat-label d-block mb-1">日期</span>
                {{ formatDate(r.reservationDate) }}
              </div>
              <div class="col-6 col-md-3">
                <span class="eat-label d-block mb-1">時間</span>
                {{ formatTime(r.reservationDate) }}
              </div>
              <div class="col-6 col-md-3">
                <span class="eat-label d-block mb-1">人數</span>
                大人 {{ r.adultsCount }}・小孩 {{ r.childrenCount }}
              </div>
              <div class="col-6 col-md-3">
                <span class="eat-label d-block mb-1">姓名</span>
                {{ r.name }}
              </div>
            </div>
          </div>
        </div>
      </section>
    </template>

    <!-- 取消確認 Dialog -->
    <div v-if="cancelTarget" class="modal-overlay" @click.self="cancelTarget = null">
      <div class="card-eat p-4" style="max-width:420px;width:90%">
        <h5 class="eat-h3 fst-normal mb-2">確認取消訂位</h5>
        <p class="eat-body-muted mb-1">訂位單號：<strong class="text-eat-primary">{{ cancelTarget.bookingNumber }}</strong></p>
        <p class="eat-body-muted mb-4">{{ formatDate(cancelTarget.reservationDate) }} {{ formatTime(cancelTarget.reservationDate) }}</p>
        <div class="d-flex gap-3 justify-content-end">
          <Button variant="secondary" @click="cancelTarget = null">返回</Button>
          <Button variant="danger" :loading="cancelling" @click="confirmCancel">確認取消</Button>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import apiFetch from '@/utils/apiFetch.js'
import { useToast } from '@/composables/useToast.js'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'
import Button from '@/components/common/Button.vue'

const { show } = useToast()

const reservations = ref([])
const loading      = ref(true)
const cancelTarget = ref(null)
const cancelling   = ref(false)

const upcoming = computed(() =>
  reservations.value.filter(r =>
    (r.status === 0 || r.status === 1) &&
    new Date(r.reservationDate) >= new Date()
  )
)

const history = computed(() =>
  reservations.value.filter(r =>
    r.status === 2 || r.status === 3 ||
    new Date(r.reservationDate) < new Date()
  )
)

function canCancel(r) {
  return r.status === 0 &&
    new Date(r.reservationDate) > new Date(Date.now() + 60 * 60 * 1000)
}

function statusClass(status) {
  return {
    0: 'status-pending',
    1: 'status-arrived',
    2: 'status-cancelled',
    3: 'status-noshow',
  }[status] ?? ''
}

function formatDate(dt) {
  return new Date(dt).toLocaleDateString('zh-TW', { year: 'numeric', month: '2-digit', day: '2-digit' })
}

function formatTime(dt) {
  return new Date(dt).toLocaleTimeString('zh-TW', { hour: '2-digit', minute: '2-digit' })
}

function openCancel(r) {
  cancelTarget.value = r
}

async function confirmCancel() {
  if (!cancelTarget.value) return
  cancelling.value = true
  try {
    const res = await apiFetch(`/Reservations/${cancelTarget.value.id}/Cancel`, {
      method: 'PUT',
      body: JSON.stringify({})
    })
    if (res.ok) {
      show('訂位已取消', 'success')
      cancelTarget.value.status = 2
      cancelTarget.value.statusText = '已取消'
      cancelTarget.value = null
    } else {
      const err = await res.json().catch(() => ({}))
      show(err.message || '取消失敗，請稍後再試', 'error')
      cancelTarget.value = null
    }
  } catch {
    show('網路錯誤，請稍後再試', 'error')
  } finally {
    cancelling.value = false
  }
}

async function fetchReservations() {
  loading.value = true
  try {
    const res = await apiFetch('/Reservations/My')
    if (res.ok) reservations.value = await res.json()
  } finally {
    loading.value = false
  }
}

onMounted(fetchReservations)
</script>

<style scoped>
.badge-status {
  font-family: var(--font-label);
  font-size: 0.7rem;
  letter-spacing: 0.12em;
  text-transform: uppercase;
  padding: 0.2rem 0.65rem;
  border-radius: var(--eat-radius-sm);
}
.status-pending   { background: rgba(227,199,107,.15); color: var(--eat-primary); border: 1px solid rgba(227,199,107,.3); }
.status-arrived   { background: rgba(60,179,113,.15);  color: #3cb371;            border: 1px solid rgba(60,179,113,.3); }
.status-cancelled { background: rgba(150,150,150,.12); color: rgba(208,197,181,.55); border: 1px solid rgba(150,150,150,.2); }
.status-noshow    { background: rgba(192,57,43,.15);   color: var(--eat-error);    border: 1px solid rgba(192,57,43,.3); }

.cancel-hint {
  font-size: .75rem;
  color: rgba(226,210,185,.4);
  letter-spacing: .02em;
}

.modal-overlay {
  position: fixed; inset: 0; z-index: 1050;
  background: rgba(0,0,0,.6); backdrop-filter: blur(4px);
  display: flex; align-items: center; justify-content: center;
}
</style>
