<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import apiFetch from '@/utils/apiFetch.js'
import { useToast } from '@/composables/useToast.js'
import Button from '@/components/common/Button.vue'

const router = useRouter()
const { show } = useToast()

const favoriteIds = ref([])
const allDishes = ref([])
const isLoading = ref(false)
const removingId = ref(null)

const favoriteDishes = computed(() =>
    allDishes.value.filter((d) => favoriteIds.value.includes(d.id))
)

async function loadData() {
    isLoading.value = true
    try {
        const [favRes, dishRes] = await Promise.all([
            apiFetch('/Favorites'),
            apiFetch('/Dishes/active'),
        ])

        if (!favRes.ok || !dishRes.ok) {
            show('載入失敗，請重新整理', 'error')
            return
        }

        const [favData, dishData] = await Promise.all([favRes.json(), dishRes.json()])

        favoriteIds.value = favData
        allDishes.value = dishData
    } catch {
        show('載入失敗，請重新整理', 'error')
    } finally {
        isLoading.value = false
    }
}

async function removeFavorite(dishId) {
    removingId.value = dishId
    const prevIds = [...favoriteIds.value]
    favoriteIds.value = favoriteIds.value.filter((id) => id !== dishId)

    try {
        const res = await apiFetch(`/Favorites/${dishId}`, { method: 'DELETE' })
        if (res.ok) {
            show('已移除收藏', 'info')
            return
        }
        favoriteIds.value = prevIds
        show('移除失敗，請再試一次', 'error')
    } catch {
        favoriteIds.value = prevIds
        show('移除失敗，請再試一次', 'error')
    } finally {
        removingId.value = null
    }
}

function viewDish(dishId) {
    router.push({ name: 'Menu', query: { dish: dishId } })
}

function firstChar(name) {
    return name ? name.charAt(0) : '?'
}

onMounted(loadData)
</script>

<template>
    <div class="d-flex flex-column">
        <div class="d-flex flex-column gap-4">
            <div>
                <h2 class="eat-h3 fst-normal mb-2">收藏餐點</h2>
                <p class="eat-body-muted mb-0">這裡會顯示會員收藏的餐點列表</p>
            </div>
            <div class="d-flex align-items-center justify-content-between mb-3">
                <p class="eat-label mb-0 fw-medium">我的收藏餐點</p>
                <span v-if="!isLoading" class="eat-label"> 共 {{ favoriteDishes.length }} 道 </span>
            </div>
        </div>

        <!-- Skeleton -->
        <template v-if="isLoading">
            <div class="fav-grid">
                <div v-for="n in 3" :key="n" class="card-eat p-4">
                    <div class="fav-card-body">
                        <div class="fav-img-wrap skeleton-box"></div>
                        <div class="fav-info d-flex flex-column gap-2">
                            <div class="skeleton-line" style="width: 55%"></div>
                            <div class="skeleton-line" style="width: 30%"></div>
                            <div class="skeleton-line" style="width: 25%"></div>
                        </div>
                    </div>
                </div>
            </div>
        </template>

        <!-- 空狀態 -->
        <div
            v-else-if="favoriteDishes.length === 0"
            class="card-eat p-4 text-center"
            style="padding-top: 3rem !important; padding-bottom: 3rem !important"
        >
            <h3 class="eat-h3 fst-normal mb-3">尚無收藏餐點</h3>
            <p class="eat-body-muted mb-4">探索我們的菜單，將喜愛的餐點加入收藏吧！</p>
            <Button variant="primary" to="/menu">前往菜單探索</Button>
        </div>

        <!-- 收藏列表 -->
        <div v-else class="fav-grid">
            <div v-for="dish in favoriteDishes" :key="dish.id" class="card-eat p-4">
                <div class="fav-card-body">
                    <!-- 縮圖 -->
                    <div class="fav-img-wrap">
                        <img
                            v-if="dish.imageUrl"
                            :src="dish.imageUrl"
                            :alt="dish.dishName"
                            class="fav-img"
                            loading="lazy"
                        />
                        <div v-else class="fav-img-placeholder eat-label">
                            {{ firstChar(dish.dishName) }}
                        </div>
                    </div>

                    <!-- 資訊 -->
                    <div class="fav-info">
                        <h3 class="eat-h3 mb-1">{{ dish.dishName }}</h3>
                        <span class="eat-label eat-label-muted d-block mb-1">{{
                            dish.categoryName
                        }}</span>
                        <span class="eat-price d-block mb-3">NT$ {{ dish.price }}</span>
                        <div class="d-flex gap-2 flex-wrap">
                            <Button
                                variant="secondary"
                                class="btn-eat-sm"
                                :disabled="removingId === dish.id"
                                @click="viewDish(dish.id)"
                            >
                                查看
                            </Button>
                            <Button
                                variant="danger"
                                class="btn-eat-sm"
                                :disabled="removingId === dish.id"
                                :loading="removingId === dish.id"
                                @click="removeFavorite(dish.id)"
                            >
                                {{ removingId === dish.id ? '移除中...' : '移除' }}
                            </Button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
/* ── 列表 Grid 容器 ── */
.fav-grid {
    display: flex;
    flex-direction: column;
    gap: 1rem;
}

/* ── 單張卡片內部版型 ── */
.fav-card-body {
    display: flex;
    flex-direction: row;
    gap: 1rem;
    align-items: flex-start;
}

/* ── 縮圖區塊 ── */
.fav-img-wrap {
    width: 120px;
    height: 120px;
    flex-shrink: 0;
    border-radius: var(--eat-radius);
    overflow: hidden;
    background: var(--eat-surface);
    display: flex;
    align-items: center;
    justify-content: center;
}

.fav-img {
    width: 100%;
    height: 100%;
    object-fit: cover;
}

.fav-img-placeholder {
    font-size: 2rem;
    color: var(--eat-on-surface-variant);
}

/* ── 資訊區塊 ── */
.fav-info {
    flex: 1;
    min-width: 0;
}

/* ── 標籤淡色 ── */
.eat-label-muted {
    color: var(--eat-on-surface-variant);
}

/* ── 單價 ── */
.eat-price {
    font-family: var(--font-label);
    font-size: 1rem;
    color: var(--eat-primary);
    font-weight: 600;
}

/* ── Skeleton shimmer ── */
.skeleton-box,
.skeleton-line {
    border-radius: var(--eat-radius);
    background: linear-gradient(
        90deg,
        var(--eat-surface) 25%,
        var(--eat-surface-container) 50%,
        var(--eat-surface) 75%
    );
    background-size: 200% 100%;
    animation: fav-shimmer 1.4s infinite;
}

.skeleton-line {
    height: 0.875rem;
    border-radius: 4px;
}

@keyframes fav-shimmer {
    0% {
        background-position: 200% 0;
    }
    100% {
        background-position: -200% 0;
    }
}

/* ── 行動版：2 欄 Grid，縮圖在上資訊在下 ── */
@media (max-width: 768px) {
    .fav-grid {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 0.75rem;
    }

    .fav-card-body {
        flex-direction: column;
    }

    .fav-img-wrap {
        width: 100%;
        height: 160px;
    }
}
</style>
