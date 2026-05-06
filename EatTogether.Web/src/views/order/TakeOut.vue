<template>
    <div class="out-wrap">
        <!-- ══ 步驟 Banner ══ -->
        <div v-if="step < 4" class="step-banner">
            <div class="step-items">
                <div :class="['step-dot', step === 1 ? 'active' : 'done']">
                    <svg
                        v-if="step > 1"
                        xmlns="http://www.w3.org/2000/svg"
                        width="12"
                        height="12"
                        fill="currentColor"
                        viewBox="0 0 16 16"
                    >
                        <path
                            d="M13.854 3.646a.5.5 0 0 1 0 .708l-7 7a.5.5 0 0 1-.708 0l-3.5-3.5a.5.5 0 1 1 .708-.708L6.5 10.293l6.646-6.647a.5.5 0 0 1 .708 0z"
                        />
                    </svg>
                    <span v-else>1</span>
                </div>
                <span :class="['step-lbl', step === 1 ? 'active' : 'done-lbl']">選餐</span>
                <div class="step-line"></div>
                <div :class="['step-dot', step === 2 ? 'active' : step > 2 ? 'done' : '']">
                    <svg
                        v-if="step > 2"
                        xmlns="http://www.w3.org/2000/svg"
                        width="12"
                        height="12"
                        fill="currentColor"
                        viewBox="0 0 16 16"
                    >
                        <path
                            d="M13.854 3.646a.5.5 0 0 1 0 .708l-7 7a.5.5 0 0 1-.708 0l-3.5-3.5a.5.5 0 1 1 .708-.708L6.5 10.293l6.646-6.647a.5.5 0 0 1 .708 0z"
                        />
                    </svg>
                    <span v-else>2</span>
                </div>
                <span :class="['step-lbl', step === 2 ? 'active' : step > 2 ? 'done-lbl' : '']"
                    >填寫資料</span
                >
                <div class="step-line"></div>
                <div :class="['step-dot', step === 3 ? 'active' : '']">3</div>
                <span :class="['step-lbl', step === 3 ? 'active' : '']">確認送出</span>
            </div>
            <!-- 取餐時段顯示 -->
            <div class="banner-right">
                <div v-if="pickupTime" class="pickup-pill">
                    <svg
                        xmlns="http://www.w3.org/2000/svg"
                        width="12"
                        height="12"
                        fill="currentColor"
                        viewBox="0 0 16 16"
                    >
                        <path
                            d="M8 3.5a.5.5 0 0 0-1 0V9a.5.5 0 0 0 .252.434l3.5 2a.5.5 0 0 0 .496-.868L8 8.71V3.5z"
                        />
                        <path
                            d="M8 16A8 8 0 1 0 8 0a8 8 0 0 0 0 16zm7-8A7 7 0 1 1 1 8a7 7 0 0 1 14 0z"
                        />
                    </svg>
                    取餐時段：<strong>{{ pickupTime }}</strong>
                </div>
            </div>
        </div>

        <!-- ══ STEP 1：選餐 ══ -->
        <div v-if="step === 1" class="out-layout">
            <!-- 手機分類捲動列 -->
            <div class="mobile-cat-bar">
                <nav class="mobile-cat-tabs">
                    <button
                        v-for="cat in sidebarCategories"
                        :key="cat.key"
                        :class="['mobile-cat-btn', { active: activeSidebarCat === cat.key }]"
                        @click="scrollToSection(cat.key)"
                    >
                        {{ cat.label }}<span class="cat-badge">{{ cat.count }}</span>
                    </button>
                </nav>
            </div>

            <!-- 左 Sidebar -->
            <aside class="out-sidebar">
                <div class="sidebar-search-wrap">
                    <svg
                        style="color: rgba(208, 197, 181, 0.4); flex-shrink: 0"
                        xmlns="http://www.w3.org/2000/svg"
                        width="13"
                        height="13"
                        fill="currentColor"
                        viewBox="0 0 16 16"
                    >
                        <path
                            d="M11.742 10.344a6.5 6.5 0 1 0-1.397 1.398h-.001c.03.04.062.078.098.115l3.85 3.85a1 1 0 0 0 1.415-1.414l-3.85-3.85a1.007 1.007 0 0 0-.115-.1zM12 6.5a5.5 5.5 0 1 1-11 0 5.5 5.5 0 0 1 11 0z"
                        />
                    </svg>
                    <input
                        v-model="searchQuery"
                        type="text"
                        class="input-line"
                        style="font-size: 0.85rem"
                        placeholder="搜尋料理名稱…"
                    />
                </div>
                <nav class="sidebar-nav">
                    <template v-for="(cat, idx) in sidebarCategories" :key="cat.key">
                        <div
                            v-if="
                                idx > 0 &&
                                !['今日推薦', '主廚特選', '我的收藏', '歷史訂單'].includes(
                                    cat.key
                                ) &&
                                ['今日推薦', '主廚特選', '我的收藏', '歷史訂單'].includes(
                                    sidebarCategories[idx - 1].key
                                )
                            "
                            class="cat-divider"
                        ></div>
                        <button
                            :class="[
                                'cat-link',
                                { active: activeSidebarCat === cat.key },
                                {
                                    'cat-special': [
                                        '今日推薦',
                                        '主廚特選',
                                        '我的收藏',
                                        '歷史訂單',
                                    ].includes(cat.key),
                                },
                            ]"
                            @click="scrollToSection(cat.key)"
                        >
                            <span>{{ cat.label }}</span>
                            <span class="cat-count">{{ cat.count }}</span>
                        </button>
                    </template>
                </nav>
            </aside>

            <!-- 中：菜單主體 -->
            <main class="out-menu" id="outMenuMain">
                <!-- Toolbar -->
                <div class="toolbar">
                    <div class="mobile-search-wrap">
                        <svg
                            style="color: rgba(208, 197, 181, 0.4); flex-shrink: 0"
                            xmlns="http://www.w3.org/2000/svg"
                            width="13"
                            height="13"
                            fill="currentColor"
                            viewBox="0 0 16 16"
                        >
                            <path
                                d="M11.742 10.344a6.5 6.5 0 1 0-1.397 1.398h-.001c.03.04.062.078.098.115l3.85 3.85a1 1 0 0 0 1.415-1.414l-3.85-3.85a1.007 1.007 0 0 0-.115-.1zM12 6.5a5.5 5.5 0 1 1-11 0 5.5 5.5 0 0 1 11 0z"
                            />
                        </svg>
                        <input
                            v-model="searchQuery"
                            type="text"
                            class="input-line"
                            style="font-size: 0.85rem"
                            placeholder="搜尋料理名稱…"
                        />
                    </div>
                    <div class="toolbar-row">
                        <div class="chips-wrap">
                            <span class="chip-label">篩選條件：</span>
                            <button
                                @click="toggleChip('veg')"
                                :class="['chip-btn', { active: activeChip === 'veg' }]"
                            >
                                素食
                            </button>
                            <button
                                @click="toggleChip('spicy')"
                                :class="['chip-btn', { active: activeChip === 'spicy' }]"
                            >
                                辣味
                            </button>
                        </div>
                        <div class="view-toggle">
                            <button
                                @click="curView = 'list'"
                                :class="['view-btn', { active: curView === 'list' }]"
                            >
                                <svg
                                    xmlns="http://www.w3.org/2000/svg"
                                    width="13"
                                    height="13"
                                    fill="currentColor"
                                    viewBox="0 0 16 16"
                                >
                                    <path
                                        fill-rule="evenodd"
                                        d="M2.5 12a.5.5 0 0 1 .5-.5h10a.5.5 0 0 1 0 1H3a.5.5 0 0 1-.5-.5zm0-4a.5.5 0 0 1 .5-.5h10a.5.5 0 0 1 0 1H3a.5.5 0 0 1-.5-.5zm0-4a.5.5 0 0 1 .5-.5h10a.5.5 0 0 1 0 1H3a.5.5 0 0 1-.5-.5z"
                                    />
                                </svg>
                            </button>
                            <button
                                @click="curView = 'grid'"
                                :class="['view-btn', { active: curView === 'grid' }]"
                            >
                                <svg
                                    xmlns="http://www.w3.org/2000/svg"
                                    width="13"
                                    height="13"
                                    fill="currentColor"
                                    viewBox="0 0 16 16"
                                >
                                    <path
                                        d="M1 2.5A1.5 1.5 0 0 1 2.5 1h3A1.5 1.5 0 0 1 7 2.5v3A1.5 1.5 0 0 1 5.5 7h-3A1.5 1.5 0 0 1 1 5.5v-3zm8 0A1.5 1.5 0 0 1 10.5 1h3A1.5 1.5 0 0 1 15 2.5v3A1.5 1.5 0 0 1 13.5 7h-3A1.5 1.5 0 0 1 9 5.5v-3zm-8 8A1.5 1.5 0 0 1 2.5 9h3A1.5 1.5 0 0 1 7 10.5v3A1.5 1.5 0 0 1 5.5 15h-3A1.5 1.5 0 0 1 1 13.5v-3zm8 0A1.5 1.5 0 0 1 10.5 9h3A1.5 1.5 0 0 1 15 10.5v3A1.5 1.5 0 0 1 13.5 15h-3A1.5 1.5 0 0 1 9 13.5v-3z"
                                    />
                                </svg>
                            </button>
                        </div>
                    </div>
                </div>

                <div v-if="loading" class="status-msg">菜單載入中…</div>
                <div v-else-if="loadError" class="status-msg" style="color: #ffb4ab">
                    {{ loadError }}
                </div>

                <div v-else class="menu-sections">
                    <!-- 歷史訂單 -->
                    <template v-if="activeSidebarCat === '歷史訂單'">
                        <h2 class="section-title">歷史訂單</h2>
                        <div
                            v-for="order in orderHistory"
                            :key="order.orderNumber"
                            class="history-card"
                        >
                            <div class="history-meta">
                                <span
                                    class="font-label"
                                    style="color: rgba(208, 197, 181, 0.5); font-size: 0.7rem"
                                    >訂單編號：{{ order.orderNumber }}</span
                                >
                                <span
                                    class="font-label"
                                    style="color: rgba(208, 197, 181, 0.5); font-size: 0.7rem"
                                >
                                    下單時間：{{
                                        new Date(order.orderAt).toLocaleString('zh-TW', {
                                            year: 'numeric',
                                            month: '2-digit',
                                            day: '2-digit',
                                            hour: '2-digit',
                                            minute: '2-digit',
                                        })
                                    }}
                                </span>
                            </div>
                            <div class="history-items">
                                <div
                                    v-for="item in order.items?.filter((i) => i)"
                                    :key="item.productName"
                                    class="font-body"
                                    style="color: #f9ddd3; font-size: 0.9rem"
                                >
                                    {{ item.qty }} x {{ item.productName }}
                                    <span
                                        v-if="item.note"
                                        style="color: rgba(208, 197, 181, 0.5); font-size: 0.8rem"
                                        >（{{ item.note }}）</span
                                    >
                                </div>
                                <div
                                    v-if="order.orderNote"
                                    class="font-body"
                                    style="
                                        color: rgba(208, 197, 181, 0.5);
                                        font-size: 0.8rem;
                                        margin-top: 0.25rem;
                                    "
                                >
                                    備註：{{ order.orderNote }}
                                </div>
                            </div>
                            <button class="history-reorder-btn font-label" @click="reorder(order)">
                                再點一次
                            </button>
                        </div>
                    </template>

                    <template v-for="section in displaySections" :key="section.key">
                        <div v-show="section.dishes.length > 0" :id="'out-sec-' + section.key">
                            <h2 class="section-title">{{ section.label }}</h2>
                            <div class="dishes-wrap" :class="{ 'grid-view': curView === 'grid' }">
                                <div
                                    v-for="dish in section.dishes"
                                    :key="dish.productId"
                                    class="dish-row mb-3"
                                    :class="{ 'dish-row-grid': curView === 'grid' }"
                                    @click="openDetail(dish)"
                                >
                                    <img
                                        v-if="dish.imageUrl && !imgErrors.has(dish.productId)"
                                        class="dish-img"
                                        :src="
                                            imgFallback.get(dish.productId) ||
                                            resolveImage(dish.imageUrl)
                                        "
                                        :alt="dish.productName"
                                        @error="handleImgError(dish)"
                                    />
                                    <div v-else class="dish-img dish-img-placeholder">🍽️</div>
                                    <div
                                        class="dish-content"
                                        :class="{ 'dish-content-grid': curView === 'grid' }"
                                    >
                                        <h3
                                            class="font-headline dish-name italic"
                                            style="color: #e3c76b"
                                        >
                                            {{ dish.productName }}
                                        </h3>
                                        <p
                                            class="font-body text-xs italic leading-relaxed"
                                            style="color: rgba(249, 221, 211, 0.5); margin: 0"
                                        >
                                            {{ dish.description }}
                                        </p>
                                        <template v-if="curView !== 'grid'">
                                            <div class="dish-badges">
                                                <span
                                                    v-if="dish.isRecommended"
                                                    class="badge badge-new"
                                                    >推薦</span
                                                >
                                                <span v-if="dish.isPopular" class="badge badge-chef"
                                                    >主廚特選</span
                                                >
                                                <span
                                                    v-if="dish.isVegetarian"
                                                    class="badge badge-veg"
                                                    >素</span
                                                >
                                                <span
                                                    v-if="dish.spicyLevel > 0"
                                                    class="badge badge-spicy"
                                                    >辣</span
                                                >
                                            </div>
                                            <div class="list-footer" @click.stop="">
                                                <p
                                                    class="font-label text-xs tracking-wider"
                                                    style="color: #d5b478"
                                                >
                                                    NT$ {{ dish.unitPrice?.toLocaleString() }}
                                                </p>
                                                <div class="qty-col">
                                                    <button
                                                        class="qty-btn"
                                                        @click.stop="handleRemove(dish)"
                                                    >
                                                        −
                                                    </button>
                                                    <span
                                                        class="font-label text-xs qty-num"
                                                        :class="{
                                                            active:
                                                                (store.cart[dish.productId] || 0) >
                                                                0,
                                                        }"
                                                        style="font-size: 20px"
                                                        >{{ store.cart[dish.productId] || 0 }}</span
                                                    >
                                                    <button
                                                        class="qty-btn"
                                                        @click.stop="openDetail(dish)"
                                                    >
                                                        +
                                                    </button>
                                                </div>
                                            </div>
                                        </template>
                                    </div>
                                    <!-- 網格視圖底部 -->
                                    <div
                                        v-if="curView === 'grid'"
                                        class="grid-footer"
                                        @click.stop=""
                                    >
                                        <div class="dish-badges" style="justify-content: center">
                                            <span v-if="dish.isRecommended" class="badge badge-new"
                                                >推薦</span
                                            >
                                            <span v-if="dish.isPopular" class="badge badge-chef"
                                                >主廚特選</span
                                            >
                                            <span v-if="dish.isVegetarian" class="badge badge-veg"
                                                >素</span
                                            >
                                            <span
                                                v-if="dish.spicyLevel > 0"
                                                class="badge badge-spicy"
                                                >辣</span
                                            >
                                        </div>
                                        <p class="font-label grid-price mb-2">
                                            NT$ {{ dish.unitPrice?.toLocaleString() }}
                                        </p>
                                        <div class="qty-row">
                                            <button
                                                class="qty-btn"
                                                @click.stop="handleRemove(dish)"
                                            >
                                                −
                                            </button>
                                            <span
                                                class="font-label text-xs qty-num"
                                                :class="{
                                                    active: (store.cart[dish.productId] || 0) > 0,
                                                }"
                                                style="font-size: 20px"
                                                >{{ store.cart[dish.productId] || 0 }}</span
                                            >
                                            <button class="qty-btn" @click.stop="openDetail(dish)">
                                                +
                                            </button>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="feather-divider my-5"></div>
                        </div>
                    </template>
                </div>
            </main>

            <!-- 右：購物車 -->
            <aside class="out-cart" :class="{ 'cart-open': cartOpen }">
                <div class="cart-header candle-glow">
                    <span
                        class="font-headline"
                        style="color: #e3c76b; font-size: 1.1rem; line-height: 1"
                        >外帶訂單</span
                    >
                    <div style="display: flex; align-items: center; gap: 0.75rem">
                        <span
                            class="font-label"
                            style="
                                color: rgba(208, 197, 181, 0.5);
                                font-size: 0.75rem;
                                letter-spacing: 0.12em;
                            "
                            >共 {{ store.totalItems }} 項</span
                        >
                        <button @click="cartOpen = false" class="cart-close-btn">✕</button>
                    </div>
                </div>

                <!-- 取餐時段 -->
                <div class="cart-pickup">
                    <label class="font-label pickup-label" :class="{ 'pickup-error': pickupError }">
                        <svg
                            xmlns="http://www.w3.org/2000/svg"
                            width="12"
                            height="12"
                            fill="currentColor"
                            viewBox="0 0 16 16"
                        >
                            <path
                                d="M8 3.5a.5.5 0 0 0-1 0V9a.5.5 0 0 0 .252.434l3.5 2a.5.5 0 0 0 .496-.868L8 8.71V3.5z"
                            />
                            <path
                                d="M8 16A8 8 0 1 0 8 0a8 8 0 0 0 0 16zm7-8A7 7 0 1 1 1 8a7 7 0 0 1 14 0z"
                            />
                        </svg>
                        取餐時段
                    </label>
                    <select
                        v-model="pickupTime"
                        :class="[
                            'pickup-select font-label',
                            { 'pickup-select-error': pickupError },
                        ]"
                        @change="pickupError = false"
                    >
                        <option value="" disabled style="padding: 0">選擇時段…</option>
                        <option v-for="slot in pickupTimeOptions" :key="slot" :value="slot">
                            {{ slot }}
                        </option>
                    </select>
                    <p v-if="pickupError" class="font-label pickup-error-msg">請先選擇取餐時段</p>
                </div>

                <!-- 餐點清單 -->
                <div class="cart-items">
                    <div
                        v-if="store.totalItems === 0"
                        style="text-align: center; padding: 3rem 1rem"
                    >
                        <p style="font-size: 2.5rem; opacity: 0.15; margin-bottom: 0.75rem">🛍️</p>
                        <p class="font-body" style="color: rgba(249, 221, 211, 0.4)">
                            尚未加入餐點
                        </p>
                        <p
                            class="font-label"
                            style="
                                color: rgba(208, 197, 181, 0.35);
                                font-size: 0.7rem;
                                letter-spacing: 0.18em;
                                text-transform: uppercase;
                                margin-top: 0.5rem;
                            "
                        >
                            點擊料理即可加入
                        </p>
                    </div>
                    <div
                        v-for="item in cartItemsWithDetails"
                        :key="item.lineId"
                        :class="['order-item', { 'setmeal-order-item': item.isSetMeal }]"
                    >
                        <div class="order-item-top">
                            <div
                                class="order-item-name-wrap"
                                @click="openCartItemEdit(item)"
                                title="點擊編輯"
                            >
                                <div v-if="item.isSetMeal" class="setmeal-badge font-label">
                                    🍱 套餐
                                </div>
                                <p class="font-headline order-item-name" style="color: #e3c76b">
                                    {{ item.productName }}
                                </p>
                                <div v-if="item.isSetMeal" class="setmeal-subitems">
                                    <span
                                        v-for="f in item.setMealData?.fixedItems"
                                        :key="'f-' + f.dishId"
                                        class="font-label setmeal-subitem"
                                        >{{ f.dishName }} × {{ f.quantity }}</span
                                    >
                                    <span
                                        v-for="s in item.setMealData?.selectedOptions"
                                        :key="'s-' + s.dishId"
                                        class="font-label setmeal-subitem"
                                        >{{ s.dishName }} × {{ s.qty }}</span
                                    >
                                </div>
                                <p v-if="item.note" class="font-body order-item-note">
                                    {{ item.note }}
                                </p>
                            </div>
                            <div class="order-item-right">
                                <span class="font-label order-item-price"
                                    >NT$ {{ (item.unitPrice * item.qty).toLocaleString() }}</span
                                >
                                <button
                                    class="qty-btn-order"
                                    @click.stop="store.removeLineItem(item.lineId)"
                                >
                                    −
                                </button>
                                <span class="font-label order-item-qty" style="color: #f9ddd3">{{
                                    item.qty
                                }}</span>
                                <button
                                    v-if="!item.isSetMeal"
                                    class="qty-btn-order"
                                    @click.stop="store.addItem(item.productId, item.note)"
                                >
                                    +
                                </button>
                                <span v-else style="width: 1.6rem"></span>
                            </div>
                        </div>
                    </div>

                    <!-- 贈品列 -->
                    <div v-if="isLoggedIn && giftCartItem" class="order-item gift-order-item">
                        <div class="order-item-top">
                            <p class="font-headline order-item-name" style="color: #e3c76b">
                                {{ giftCartItem.productName }}
                            </p>
                            <div class="order-item-right">
                                <span class="gift-order-badge font-label">🎁 贈品</span>
                                <span
                                    class="font-label order-item-qty"
                                    style="color: rgba(208, 197, 181, 0.55); font-size: 0.75rem"
                                    >× 1</span
                                >
                            </div>
                        </div>
                    </div>
                </div>

                <!-- 購物車底部 -->
                <div class="cart-footer">
                    <div
                        style="
                            padding: 0.75rem 1rem;
                            border-bottom: 1px solid rgba(77, 70, 58, 0.2);
                        "
                    >
                        <textarea
                            v-model="store.specialRequest"
                            class="note-textarea font-body resize-none"
                            rows="2"
                            placeholder="備註：過敏食材、特殊需求…"
                        ></textarea>

                        <!-- 優惠券：已登入顯示下拉，未登入顯示文字輸入 -->
                        <div style="margin-top: 0.5rem">
                            <!-- 已登入：下拉選已領優惠券 -->
                            <template v-if="isLoggedIn">
                                <select
                                    v-model="selectedCouponCode"
                                    @change="onCouponSelect"
                                    class="coupon-select font-body"
                                >
                                    <option value="">— 選擇優惠券 —</option>
                                    <option
                                        v-for="c in availableCoupons"
                                        :key="c.id"
                                        :value="c.code"
                                    >
                                        {{ c.couponName }}｜{{ c.discountDescription }}
                                        <template v-if="c.endDate">
                                            （{{ new Date(c.endDate).toLocaleDateString('zh-TW') }}
                                            到期）
                                        </template>
                                    </option>
                                    <!-- 最後一項：導到優惠券專區 -->
                                    <option value="__coupons__" class="coupon-goto-option">
                                        ✦ 查看更多優惠券 →
                                    </option>
                                </select>
                                <p
                                    v-if="couponMsg"
                                    class="font-label mb-0"
                                    style="font-size: 0.7rem; margin-top: 0.25rem"
                                    :style="{ color: couponOk ? '#a3d977' : '#ffb4ab' }"
                                >
                                    {{ couponMsg }}
                                </p>
                            </template>

                            <!-- 未登入：提示登入才能使用優惠券 -->
                            <template v-else>
                                <button @click="openAuthModal" class="font-label coupon-login-btn">
                                    立即登入會員享優惠
                                </button>
                            </template>
                        </div>
                    </div>

                    <!-- 合計區 -->
                    <div
                        style="
                            padding: 0.75rem 1rem;
                            display: flex;
                            flex-direction: column;
                            gap: 0.5rem;
                        "
                    >
                        <div style="display: flex; flex-direction: column; gap: 0.3rem">
                            <!-- 已套用活動標籤 -->
                            <div v-if="isLoggedIn && bestAutoEvent" class="applied-event-tag">
                                <span class="applied-event-icon">🎉</span>
                                <span class="font-label applied-event-text"
                                    >已參加「{{ bestAutoEvent.title }}」活動 （滿 NT${{
                                        bestAutoEvent.minSpend
                                    }}
                                    可享 {{ bestAutoEvent.discountDescription }}）</span
                                >
                            </div>
                            <!-- 活動折扣 -->
                            <div
                                v-if="isLoggedIn && autoEventDiscount > 0"
                                style="
                                    display: flex;
                                    justify-content: space-between;
                                    align-items: center;
                                "
                            >
                                <span
                                    class="font-label"
                                    style="
                                        font-size: 0.8rem;
                                        color: rgba(163, 217, 119, 0.8);
                                        letter-spacing: 0.06em;
                                    "
                                    >活動折扣</span
                                >
                                <span
                                    class="font-label"
                                    style="
                                        font-size: 0.8rem;
                                        color: #a3d977;
                                        letter-spacing: 0.06em;
                                    "
                                    >－ NT$ {{ autoEventDiscount.toLocaleString() }}</span
                                >
                            </div>
                            <!-- 優惠券折扣 -->
                            <div
                                v-if="couponOk && couponDiscount > 0"
                                style="
                                    display: flex;
                                    justify-content: space-between;
                                    align-items: center;
                                "
                            >
                                <span
                                    class="font-label"
                                    style="
                                        font-size: 0.8rem;
                                        color: rgba(163, 217, 119, 0.8);
                                        letter-spacing: 0.06em;
                                    "
                                    >優惠券折扣</span
                                >
                                <span
                                    class="font-label"
                                    style="
                                        font-size: 0.8rem;
                                        color: #a3d977;
                                        letter-spacing: 0.06em;
                                    "
                                    >－ NT$ {{ couponDiscount.toLocaleString() }}</span
                                >
                            </div>
                            <!-- 合計 -->
                            <div
                                style="
                                    display: flex;
                                    justify-content: space-between;
                                    align-items: center;
                                "
                            >
                                <span
                                    class="font-headline"
                                    style="color: #e3c76b; font-size: 1.1rem"
                                    >合計</span
                                >
                                <span
                                    class="font-label"
                                    style="
                                        color: #e3c76b;
                                        font-size: 1.1rem;
                                        letter-spacing: 0.08em;
                                    "
                                    >NT$ {{ finalTotal.toLocaleString() }}</span
                                >
                            </div>
                        </div>
                        <button
                            @click="goToStep2"
                            :disabled="store.totalItems === 0"
                            class="submit-btn font-label"
                            style="
                                display: block;
                                width: 100%;
                                padding: 0.875rem 0;
                                font-size: 1rem;
                                letter-spacing: 0.28em;
                                text-transform: uppercase;
                            "
                        >
                            下一步：填寫資料
                        </button>
                        <button
                            @click="clearAll"
                            class="clear-btn font-label"
                            style="
                                display: block;
                                width: 100%;
                                padding: 0.625rem 0;
                                font-size: 0.85rem;
                                letter-spacing: 0.1em;
                                text-transform: uppercase;
                            "
                        >
                            清空訂單
                        </button>
                    </div>
                </div>
            </aside>
        </div>

        <!-- 手機底部列（Step 1） -->
        <div v-if="step === 1" class="mobile-bottom-bar" @click="cartOpen = true">
            <div style="display: flex; align-items: center; gap: 0.75rem">
                <span class="bottom-badge">{{ store.totalItems }}</span>
                <span
                    class="font-label text-xs tracking-widest uppercase"
                    style="color: rgba(208, 197, 181, 0.7)"
                    >查看訂單</span
                >
            </div>
            <div style="display: flex; align-items: center; gap: 0.75rem">
                <span class="font-label text-sm" style="color: #e3c76b"
                    >NT$ {{ finalTotal.toLocaleString() }}</span
                >
                <span
                    class="bottom-cta font-label text-xs"
                    style="letter-spacing: 0.2em; text-transform: uppercase"
                    >下一步</span
                >
            </div>
        </div>
        <div v-if="cartOpen" @click="cartOpen = false" class="mobile-overlay"></div>

        <!-- ══ STEP 2：填寫資料 ══ -->
        <div v-else-if="step === 2" class="step-page">
            <div class="step-card">
                <h2 class="step-card-title font-headline">填寫取餐資料</h2>

                <div class="form-group">
                    <label class="form-label font-label" :class="{ 'form-label-error': nameError }">
                        姓名 <span class="required-mark">*</span>
                        <span v-if="isLoggedIn" class="autofill-hint font-label"
                            >已帶入會員資料</span
                        >
                    </label>
                    <input
                        v-model="customerName"
                        type="text"
                        :class="['form-input font-body', { 'input-error': nameError }]"
                        placeholder="請輸入取餐人姓名"
                        @input="nameError = false"
                    />
                    <p v-if="nameError" class="form-error-msg font-label">請填寫姓名</p>
                </div>

                <div class="form-group">
                    <label
                        class="form-label font-label"
                        :class="{ 'form-label-error': phoneError }"
                    >
                        電話 <span class="required-mark">*</span>
                    </label>
                    <input
                        v-model="customerPhone"
                        type="tel"
                        :class="['form-input font-body', { 'input-error': phoneError }]"
                        placeholder="09xx-xxxxxx"
                        @input="phoneError = false"
                    />
                    <p v-if="phoneError" class="form-error-msg font-label">請填寫有效電話號碼</p>
                </div>

                <div class="form-group">
                    <label class="form-label font-label">取餐時段</label>
                    <div class="pickup-display font-label">
                        <svg
                            xmlns="http://www.w3.org/2000/svg"
                            width="13"
                            height="13"
                            fill="currentColor"
                            viewBox="0 0 16 16"
                        >
                            <path
                                d="M8 3.5a.5.5 0 0 0-1 0V9a.5.5 0 0 0 .252.434l3.5 2a.5.5 0 0 0 .496-.868L8 8.71V3.5z"
                            />
                            <path
                                d="M8 16A8 8 0 1 0 8 0a8 8 0 0 0 0 16zm7-8A7 7 0 1 1 1 8a7 7 0 0 1 14 0z"
                            />
                        </svg>
                        今日 {{ pickupTime }}
                    </div>
                </div>

                <div class="form-group">
                    <label class="form-label font-label">備註（選填）</label>
                    <textarea
                        v-model="store.specialRequest"
                        class="form-textarea font-body"
                        rows="3"
                        placeholder="過敏食材、特殊需求…"
                    ></textarea>
                </div>

                <div class="form-group utensils-group">
                    <label class="utensils-label font-label">
                        <input v-model="needUtensils" type="checkbox" class="utensils-checkbox" />
                        <span class="utensils-text">需要餐具</span>
                    </label>
                </div>

                <!-- 小計摘要 -->
                <div class="step2-summary">
                    <div style="display: flex; justify-content: space-between">
                        <span
                            class="font-label"
                            style="color: rgba(208, 197, 181, 0.6); font-size: 0.8rem"
                            >小計</span
                        >
                        <span class="font-label" style="font-size: 0.8rem"
                            >NT$ {{ total.toLocaleString() }}</span
                        >
                    </div>
                    <div
                        v-if="isLoggedIn && autoEventDiscount > 0"
                        style="display: flex; justify-content: space-between"
                    >
                        <span class="font-label" style="color: #a3d977; font-size: 0.8rem"
                            >活動折扣</span
                        >
                        <span class="font-label" style="color: #a3d977; font-size: 0.8rem"
                            >－ NT$ {{ autoEventDiscount.toLocaleString() }}</span
                        >
                    </div>
                    <div
                        v-if="couponOk && couponDiscount > 0"
                        style="display: flex; justify-content: space-between"
                    >
                        <span class="font-label" style="color: #a3d977; font-size: 0.8rem"
                            >優惠券折扣</span
                        >
                        <span class="font-label" style="color: #a3d977; font-size: 0.8rem"
                            >－ NT$ {{ couponDiscount.toLocaleString() }}</span
                        >
                    </div>
                    <div class="step2-total-row">
                        <span class="font-headline" style="color: #e3c76b">應付金額</span>
                        <span
                            class="font-label"
                            style="color: #e3c76b; font-size: 1.15rem; letter-spacing: 0.08em"
                            >NT$ {{ finalTotal.toLocaleString() }}</span
                        >
                    </div>
                </div>

                <div class="step-nav">
                    <button @click="step = 1" class="step-back-btn font-label">← 返回選餐</button>
                    <button @click="goToStep3" class="submit-btn font-label step-next-btn">
                        確認資料 →
                    </button>
                </div>
            </div>
        </div>

        <!-- ══ STEP 3：確認送出 ══ -->
        <div v-else-if="step === 3" class="step-page">
            <div class="step-card">
                <h2 class="step-card-title font-headline">確認訂單</h2>

                <!-- 取餐資訊 -->
                <div class="confirm-info">
                    <div class="confirm-info-row">
                        <span class="font-label confirm-info-label">取餐人</span>
                        <span class="font-body confirm-info-val">{{ customerName }}</span>
                    </div>
                    <div class="confirm-info-row">
                        <span class="font-label confirm-info-label">電話</span>
                        <span class="font-body confirm-info-val">{{ customerPhone }}</span>
                    </div>
                    <div class="confirm-info-row">
                        <span class="font-label confirm-info-label">取餐時段</span>
                        <span class="font-body confirm-info-val" style="color: #e3c76b"
                            >今日 {{ pickupTime }}</span
                        >
                    </div>
                    <div v-if="store.specialRequest" class="confirm-info-row">
                        <span class="font-label confirm-info-label">備註</span>
                        <span class="font-body confirm-info-val">{{ store.specialRequest }}</span>
                    </div>
                    <div class="confirm-info-row">
                        <span class="font-label confirm-info-label">餐具</span>
                        <span class="font-body confirm-info-val">{{
                            needUtensils ? '需要' : '不需要'
                        }}</span>
                    </div>
                </div>

                <div class="feather-divider" style="margin: 1.25rem 0"></div>

                <!-- 品項明細 -->
                <h3
                    class="font-label"
                    style="
                        color: rgba(208, 197, 181, 0.5);
                        font-size: 0.9rem;
                        letter-spacing: 0.18em;
                        text-transform: uppercase;
                        margin-bottom: 0.75rem;
                    "
                >
                    品項明細
                </h3>
                <div class="confirm-items">
                    <template v-for="item in cartItemsWithDetails" :key="item.lineId">
                        <div class="confirm-item">
                            <div class="confirm-item-left">
                                <div
                                    v-if="item.isSetMeal"
                                    class="setmeal-badge font-label"
                                    style="margin-bottom: 0.15rem"
                                >
                                    🍱 套餐
                                </div>
                                <span class="font-body confirm-item-name">{{
                                    item.productName
                                }}</span>
                                <div
                                    v-if="item.isSetMeal"
                                    class="setmeal-subitems"
                                    style="margin-top: 0.2rem"
                                >
                                    <span
                                        v-for="f in item.setMealData?.fixedItems"
                                        :key="'cf-' + f.dishId"
                                        class="font-label setmeal-subitem"
                                        >{{ f.dishName }} × {{ f.quantity }}</span
                                    >
                                    <span
                                        v-for="s in item.setMealData?.selectedOptions"
                                        :key="'cs-' + s.dishId"
                                        class="font-label setmeal-subitem"
                                        >{{ s.dishName }} × {{ s.qty }}</span
                                    >
                                </div>
                                <p
                                    v-if="item.note"
                                    class="font-label"
                                    style="
                                        color: rgba(208, 197, 181, 0.45);
                                        font-size: 0.8rem;
                                        margin: 0.1rem 0 0;
                                    "
                                >
                                    {{ item.note }}
                                </p>
                            </div>
                            <div class="confirm-item-right">
                                <span
                                    class="font-label"
                                    style="color: rgba(208, 197, 181, 0.5); font-size: 0.85rem"
                                    >× {{ item.qty }}</span
                                >
                                <span class="font-label" style="color: #d5b478; font-size: 0.9rem"
                                    >NT$ {{ (item.unitPrice * item.qty).toLocaleString() }}</span
                                >
                            </div>
                        </div>
                    </template>
                    <!-- 贈品 -->
                    <div
                        v-if="isLoggedIn && giftCartItem"
                        class="confirm-item"
                        style="background: rgba(163, 217, 119, 0.04)"
                    >
                        <span class="font-body confirm-item-name">{{
                            giftCartItem.productName
                        }}</span>
                        <div class="confirm-item-right">
                            <span class="gift-order-badge font-label">🎁 贈品</span>
                        </div>
                    </div>
                </div>

                <div class="feather-divider" style="margin: 1.25rem 0"></div>

                <!-- 金額明細 -->
                <div class="confirm-totals">
                    <!-- 活動（非贈品型才顯示折抵行） -->
                    <div
                        v-if="isLoggedIn && bestAutoEvent && bestAutoEvent.discountType !== 'Gift'"
                        class="confirm-total-row-3"
                    >
                        <span class="font-label confirm-dim">活動</span>
                        <span class="font-label confirm-mid">{{ bestAutoEvent.title }}</span>
                        <span class="font-label confirm-green"
                            >折抵 NT$ {{ autoEventDiscount.toLocaleString() }}</span
                        >
                    </div>
                    <!-- 贈品型活動：只顯示參加提示 -->
                    <div
                        v-if="isLoggedIn && bestAutoEvent && bestAutoEvent.discountType === 'Gift'"
                        class="confirm-total-row"
                    >
                        <span class="font-label confirm-dim" style="font-size: 0.8rem"
                            >已參加「{{ bestAutoEvent.title }}」</span
                        >
                        <span></span>
                    </div>
                    <!-- 優惠券 -->
                    <div v-if="couponOk && couponDiscount > 0" class="confirm-total-row-3">
                        <span class="font-label confirm-dim">優惠券</span>
                        <span class="font-label confirm-mid">{{ couponCode }}</span>
                        <span class="font-label confirm-green"
                            >折抵 NT$ {{ couponDiscount.toLocaleString() }}</span
                        >
                    </div>
                    <!-- 合計（原價） -->
                    <div class="confirm-total-row">
                        <span class="font-label confirm-dim">合計</span>
                        <span class="font-label">NT$ {{ total.toLocaleString() }}</span>
                    </div>
                    <!-- 折扣（有折扣才顯示） -->
                    <div
                        v-if="autoEventDiscount + (couponOk ? couponDiscount : 0) > 0"
                        class="confirm-total-row"
                    >
                        <span class="font-label confirm-dim">折扣</span>
                        <span class="font-label confirm-green"
                            >－ NT$
                            {{
                                (
                                    autoEventDiscount + (couponOk ? couponDiscount : 0)
                                ).toLocaleString()
                            }}</span
                        >
                    </div>
                    <!-- 應付金額 -->
                    <div class="confirm-total-row confirm-grand">
                        <span class="font-headline" style="color: #e3c76b; font-size: 1.15rem"
                            >應付金額</span
                        >
                        <span
                            class="font-label"
                            style="color: #e3c76b; font-size: 1.2rem; letter-spacing: 0.08em"
                            >NT$ {{ finalTotal.toLocaleString() }}</span
                        >
                    </div>
                </div>

                <div class="step-nav" style="margin-top: 1.5rem">
                    <button @click="step = 2" class="step-back-btn font-label">← 返回修改</button>
                    <button
                        @click="submitOrder"
                        :disabled="submitting"
                        class="submit-btn font-label step-next-btn"
                    >
                        {{ submitting ? '送出中…' : '確認送出' }}
                    </button>
                </div>
            </div>
        </div>

        <!-- ══ STEP 4：訂單成功頁 ══ -->
        <div v-else-if="step === 4" class="sp-wrap">
            <!-- ① 頂部橫排：✓圓 | 訂單已送出！ | 訂單編號 -->
            <div class="sp-header-row">
                <div class="sp-check-ring">
                    <!-- 訂單 / 收據圖示 -->
                    <svg
                        xmlns="http://www.w3.org/2000/svg"
                        width="30"
                        height="30"
                        fill="currentColor"
                        viewBox="0 0 16 16"
                    >
                        <path
                            d="M1.92.506a.5.5 0 0 1 .434.14L3 1.293l.646-.647a.5.5 0 0 1 .708 0L5 1.293l.646-.647a.5.5 0 0 1 .708 0L7 1.293l.646-.647a.5.5 0 0 1 .708 0L9 1.293l.646-.647a.5.5 0 0 1 .708 0l.646.647.646-.647a.5.5 0 0 1 .708 0l.646.647.646-.647a.5.5 0 0 1 .801.13l.5 1A.5.5 0 0 1 15 2v12a.5.5 0 0 1-.053.224l-.5 1a.5.5 0 0 1-.8.13L13 14.707l-.646.647a.5.5 0 0 1-.708 0L11 14.707l-.646.647a.5.5 0 0 1-.708 0L9 14.707l-.646.647a.5.5 0 0 1-.708 0L7 14.707l-.646.647a.5.5 0 0 1-.708 0L5 14.707l-.646.647a.5.5 0 0 1-.708 0L3 14.707l-.646.647a.5.5 0 0 1-.801-.13l-.5-1A.5.5 0 0 1 1 14V2a.5.5 0 0 1 .053-.224l.5-1a.5.5 0 0 1 .367-.27zm.217 1.338L2 2.118v11.764l.137.274.51-.51a.5.5 0 0 1 .707 0l.646.647.646-.646a.5.5 0 0 1 .708 0l.646.646.646-.646a.5.5 0 0 1 .708 0l.646.646.646-.646a.5.5 0 0 1 .708 0l.646.646.646-.646a.5.5 0 0 1 .708 0l.646.646.646-.646a.5.5 0 0 1 .708 0l.509.509.137-.274V2.118l-.137-.274-.51.51a.5.5 0 0 1-.707 0L12 1.707l-.646.647a.5.5 0 0 1-.708 0L10 1.707l-.646.647a.5.5 0 0 1-.708 0L8 1.707l-.646.647a.5.5 0 0 1-.708 0L6 1.707l-.646.647a.5.5 0 0 1-.708 0L4 1.707l-.646.647a.5.5 0 0 1-.708 0l-.509-.51z"
                        />
                        <path
                            d="M3 4.5a.5.5 0 0 1 .5-.5h6a.5.5 0 1 1 0 1h-6a.5.5 0 0 1-.5-.5zm0 2a.5.5 0 0 1 .5-.5h6a.5.5 0 1 1 0 1h-6a.5.5 0 0 1-.5-.5zm0 2a.5.5 0 0 1 .5-.5h6a.5.5 0 1 1 0 1h-6a.5.5 0 0 1-.5-.5zm0 2a.5.5 0 0 1 .5-.5h6a.5.5 0 0 1 0 1h-6a.5.5 0 0 1-.5-.5zm8-6a.5.5 0 0 1 .5-.5h1a.5.5 0 0 1 0 1h-1a.5.5 0 0 1-.5-.5zm0 2a.5.5 0 0 1 .5-.5h1a.5.5 0 0 1 0 1h-1a.5.5 0 0 1-.5-.5zm0 2a.5.5 0 0 1 .5-.5h1a.5.5 0 0 1 0 1h-1a.5.5 0 0 1-.5-.5zm0 2a.5.5 0 0 1 .5-.5h1a.5.5 0 0 1 0 1h-1a.5.5 0 0 1-.5-.5z"
                        />
                    </svg>
                </div>
                <h1 class="font-headline sp-main-title">訂單已送出！</h1>
                <div class="sp-order-num-block">
                    <span class="font-label sp-order-num-label">訂單編號</span>
                    <strong class="sp-order-num-val">{{ orderNumber }}</strong>
                </div>
            </div>

            <!-- ② 動態進度條（全寬） -->
            <div class="sp-progress-bar">
                <div :class="['sp-prog-step', orderProgress >= 1 ? 'sp-prog-done' : '']">
                    <div class="sp-prog-dot">
                        <svg
                            v-if="orderProgress >= 1"
                            xmlns="http://www.w3.org/2000/svg"
                            width="16"
                            height="16"
                            fill="currentColor"
                            viewBox="0 0 16 16"
                        >
                            <path
                                d="M13.854 3.646a.5.5 0 0 1 0 .708l-7 7a.5.5 0 0 1-.708 0l-3.5-3.5a.5.5 0 1 1 .708-.708L6.5 10.293l6.646-6.647a.5.5 0 0 1 .708 0z"
                            />
                        </svg>
                        <span v-else>1</span>
                    </div>
                    <span class="font-label sp-prog-lbl">訂單已接收</span>
                </div>
                <div :class="['sp-prog-line', orderProgress >= 2 ? 'sp-prog-line-lit' : '']"></div>
                <div :class="['sp-prog-step', orderProgress >= 2 ? 'sp-prog-active' : '']">
                    <div class="sp-prog-dot">
                        <svg
                            v-if="orderProgress >= 3"
                            xmlns="http://www.w3.org/2000/svg"
                            width="16"
                            height="16"
                            fill="currentColor"
                            viewBox="0 0 16 16"
                        >
                            <path
                                d="M13.854 3.646a.5.5 0 0 1 0 .708l-7 7a.5.5 0 0 1-.708 0l-3.5-3.5a.5.5 0 1 1 .708-.708L6.5 10.293l6.646-6.647a.5.5 0 0 1 .708 0z"
                            />
                        </svg>
                        <span v-else>2</span>
                    </div>
                    <span class="font-label sp-prog-lbl">餐點製作中</span>
                </div>
                <div :class="['sp-prog-line', orderProgress >= 3 ? 'sp-prog-line-lit' : '']"></div>
                <div :class="['sp-prog-step', orderProgress >= 3 ? 'sp-prog-done' : '']">
                    <div class="sp-prog-dot"><span>3</span></div>
                    <span class="font-label sp-prog-lbl">餐點已完成</span>
                </div>
            </div>

            <!-- ③ 雙欄主體 -->
            <div class="sp-body">
                <!-- 左欄：餐點明細 -->
                <div class="sp-col-left">
                    <section class="sp-section">
                        <h3 class="font-label sp-section-title">餐點明細</h3>
                        <div class="sp-items">
                            <template v-for="item in confirmedItems" :key="item.lineId">
                                <div class="sp-item">
                                    <div class="sp-item-left">
                                        <div
                                            v-if="item.isSetMeal"
                                            class="setmeal-badge font-label"
                                            style="margin-bottom: 0.15rem"
                                        >
                                            🍱 套餐
                                        </div>
                                        <span class="font-body sp-item-name">{{
                                            item.productName
                                        }}</span>
                                        <div
                                            v-if="item.isSetMeal"
                                            class="setmeal-subitems"
                                            style="margin-top: 0.2rem"
                                        >
                                            <span
                                                v-for="f in item.setMealData?.fixedItems"
                                                :key="'sf-' + f.dishId"
                                                class="font-label setmeal-subitem"
                                                >{{ f.dishName }} × {{ f.quantity }}</span
                                            >
                                            <span
                                                v-for="s in item.setMealData?.selectedOptions"
                                                :key="'ss-' + s.dishId"
                                                class="font-label setmeal-subitem"
                                                >{{ s.dishName }} × {{ s.qty }}</span
                                            >
                                        </div>
                                        <p v-if="item.note" class="font-label sp-item-note">
                                            {{ item.note }}
                                        </p>
                                    </div>
                                    <div class="sp-item-right">
                                        <span class="font-label sp-item-qty">× {{ item.qty }}</span>
                                        <span class="font-label sp-item-price"
                                            >NT$
                                            {{ (item.unitPrice * item.qty).toLocaleString() }}</span
                                        >
                                    </div>
                                </div>
                            </template>
                            <!-- 贈品 -->
                            <div
                                v-if="confirmedGift"
                                class="sp-item"
                                style="background: rgba(163, 217, 119, 0.04)"
                            >
                                <span class="font-body sp-item-name">{{
                                    confirmedGift.productName
                                }}</span>
                                <span class="gift-order-badge font-label">🎁 贈品</span>
                            </div>
                            <!-- 贈品活動來源（贈品型活動才顯示） -->
                            <div
                                v-if="
                                    confirmedGift &&
                                    confirmedEventTitle &&
                                    confirmedEventDiscountType === 'Gift'
                                "
                                class="sp-gift-event-note font-label"
                            >
                                {{ confirmedEventTitle }}：{{ confirmedEventDesc }}
                            </div>
                        </div>

                        <div class="feather-divider" style="margin: 1rem 0 0.85rem"></div>

                        <div class="sp-meta-rows">
                            <!-- 活動（非贈品型才顯示折抵行） -->
                            <div
                                v-if="confirmedEventTitle && confirmedEventDiscountType !== 'Gift'"
                                class="sp-meta-row-3"
                            >
                                <span class="font-label sp-meta-label">活動</span>
                                <span class="font-label sp-meta-val-mid">{{
                                    confirmedEventTitle
                                }}</span>
                                <span class="font-label sp-meta-discount"
                                    >折抵 NT$
                                    {{ confirmedAutoEventDiscount.toLocaleString() }}</span
                                >
                            </div>
                            <!-- 優惠券 -->
                            <div v-if="confirmedCouponCode" class="sp-meta-row-3">
                                <span class="font-label sp-meta-label">優惠券</span>
                                <span class="font-label sp-meta-val-mid">{{
                                    confirmedCouponCode
                                }}</span>
                                <span class="font-label sp-meta-discount"
                                    >折抵 NT$ {{ confirmedCouponDiscount.toLocaleString() }}</span
                                >
                            </div>
                            <!-- 合計（原價） -->
                            <div class="sp-meta-row">
                                <span class="font-label sp-meta-label">合計</span>
                                <span class="font-label sp-meta-val"
                                    >NT$ {{ confirmedSubtotal.toLocaleString() }}</span
                                >
                            </div>
                            <!-- 折扣（有折扣才顯示） -->
                            <div
                                v-if="confirmedAutoEventDiscount + confirmedCouponDiscount > 0"
                                class="sp-meta-row"
                            >
                                <span class="font-label sp-meta-label">折扣</span>
                                <span class="font-label sp-meta-val" style="color: #7ec87e"
                                    >－ NT$
                                    {{
                                        (
                                            confirmedAutoEventDiscount + confirmedCouponDiscount
                                        ).toLocaleString()
                                    }}</span
                                >
                            </div>
                        </div>

                        <!-- 金額總計（突出顯示） -->
                        <div class="feather-divider" style="margin: 0.75rem 0 0.65rem"></div>
                        <div class="sp-meta-row">
                            <span class="font-label sp-meta-label" style="font-size: 1rem"
                                >金額總計</span
                            >
                            <span class="font-label sp-meta-gold"
                                >NT$ {{ confirmedTotal.toLocaleString() }}</span
                            >
                        </div>

                        <!-- 備註（金額總計下方） -->
                        <div v-if="confirmedNote" class="sp-meta-row" style="margin-top: 0.5rem">
                            <span class="font-label sp-meta-label">備註</span>
                            <span class="font-label sp-meta-val">{{ confirmedNote }}</span>
                        </div>
                    </section>
                </div>

                <!-- 右欄：提醒 + 取餐 + 聯絡 + 返回 -->
                <div class="sp-col-right">
                    <!-- 防呆提醒（最上方）-->
                    <div class="sp-reminders">
                        <div class="sp-reminder-item">
                            <span class="sp-reminder-icon">⏱</span>
                            <span class="font-label">餐點現點現做，請耐心等候</span>
                        </div>
                        <div class="sp-reminder-item">
                            <span class="sp-reminder-icon">🍱</span>
                            <span class="font-label">請於完成後 15 分鐘內取餐，以確保最佳風味</span>
                        </div>
                        <div class="sp-reminder-item">
                            <span class="sp-reminder-icon">🪙</span>
                            <span class="font-label">取餐時請告知訂單編號或出示本頁面</span>
                        </div>
                    </div>

                    <!-- 預計取餐時間 -->
                    <div class="sp-pickup-banner">
                        <!-- 上排：標籤 + 時間 -->
                        <div class="sp-pickup-top">
                            <span class="font-label sp-pickup-label">預計取餐時間</span>
                            <span class="font-headline sp-pickup-time"
                                >今日 {{ confirmedPickupTime }}</span
                            >
                        </div>
                        <!-- 下排：取餐人資訊 + 付款 / 取餐方式 -->
                        <div class="sp-pickup-meta">
                            <div v-if="confirmedName" class="sp-meta-row">
                                <span class="font-label sp-meta-label">取餐人</span>
                                <span class="font-label sp-meta-val">{{ confirmedName }}</span>
                            </div>
                            <div v-if="confirmedPhone" class="sp-meta-row">
                                <span class="font-label sp-meta-label">聯絡電話</span>
                                <span class="font-label sp-meta-val">{{ confirmedPhone }}</span>
                            </div>
                            <div class="sp-meta-row">
                                <span class="font-label sp-meta-label">付款方式</span>
                                <span class="font-label sp-meta-val">現場付款</span>
                            </div>
                            <div class="sp-meta-row">
                                <span class="font-label sp-meta-label">取餐方式</span>
                                <span class="font-label sp-meta-val">臨櫃自取</span>
                            </div>
                            <div class="sp-meta-row">
                                <span class="font-label sp-meta-label">餐具</span>
                                <span class="font-label sp-meta-val">{{ confirmedNeedUtensils ? '需要' : '不需要' }}</span>
                            </div>
                        </div>
                    </div>

                    <!-- 聯絡資訊 -->
                    <section class="sp-section">
                        <h3 class="font-label sp-section-title">餐廳聯絡資訊</h3>
                        <div class="sp-store-info">
                            <p class="font-body sp-store-addr">台北市大安區慢食街 88 號</p>
                            <a href="tel:0223456789" class="font-body sp-store-tel"
                                >Tel: (02) 2345-6789</a
                            >
                        </div>
                    </section>

                    <!-- 返回菜單 -->
                    <button @click="onSuccessClose" class="sp-back-btn font-label">返回菜單</button>
                </div>
            </div>
        </div>

        <!-- ══ Detail Modal ══ -->
        <DishDetailModal
            :dish="activeDetail"
            :edit-mode="editingLineId !== null"
            :initial-qty="
                editingLineId !== null
                    ? (cartItemsWithDetails.find((i) => i.lineId === editingLineId)?.qty ?? 1)
                    : 1
            "
            :initial-note="
                editingLineId !== null
                    ? (cartItemsWithDetails.find((i) => i.lineId === editingLineId)?.note ?? '')
                    : ''
            "
            @close="((activeDetail = null), (editingLineId = null))"
            @confirm="onDetailConfirm"
        />

        <!-- 套餐選項 Modal -->
        <SetMealSelectModal
            :meal="activeMeal"
            :edit-mode="editingMealLineId !== null"
            :initial-qty="
                editingMealLineId !== null
                    ? (store.lines.find((l) => l.lineId === editingMealLineId)?.qty ?? 1)
                    : 1
            "
            :initial-note="
                editingMealLineId !== null
                    ? (store.lines.find((l) => l.lineId === editingMealLineId)?.note ?? '')
                    : ''
            "
            :initial-sel="editingMealLineId !== null ? buildInitialSel(editingMealLineId) : {}"
            @close="((activeMeal = null), (editingMealLineId = null))"
            @confirm="onSetMealConfirm"
        />

        <!-- 成功 Modal 已移除，改為 step 4 整頁呈現 -->

        <!-- 優惠券 × 活動衝突警告 Modal -->
        <Teleport to="body">
            <Transition name="cew-fade">
                <div v-if="couponEventWarnModal" class="cew-overlay" @click.self="cancelCouponWarn">
                    <div class="cew-box">
                        <div class="cew-icon">⚠️</div>
                        <p class="font-headline cew-title">注意</p>
                        <p class="font-label cew-body">
                            使用此優惠券後，需要再消費
                            <strong class="cew-highlight">
                                NT$
                                {{
                                    (bestAutoEvent
                                        ? bestAutoEvent.minSpend -
                                          (total - (pendingCouponData?.discount ?? 0))
                                        : 0
                                    ).toLocaleString()
                                }}
                            </strong>
                            才可享「<strong class="cew-highlight">{{ bestAutoEvent?.title }}</strong
                            >」 活動優惠（{{ bestAutoEvent?.discountDescription }}）。
                        </p>
                        <p class="font-label cew-sub">確定要使用優惠券嗎？</p>
                        <div class="cew-btns">
                            <button class="cew-btn-cancel font-label" @click="cancelCouponWarn">
                                取消
                            </button>
                            <button
                                class="cew-btn-confirm font-label"
                                @click="confirmCouponDespiteEvent"
                            >
                                確定使用
                            </button>
                        </div>
                    </div>
                </div>
            </Transition>
        </Teleport>

        <!-- 簡易 Toast -->
        <Teleport to="body">
            <div
                class="simple-toast"
                :style="
                    toastVisible
                        ? 'opacity:1;transform:translateX(-50%) translateY(0)'
                        : 'opacity:0;pointer-events:none;transform:translateX(-50%) translateY(10px)'
                "
            >
                <div
                    class="px-5 py-3 shadow-xl flex items-center gap-3"
                    style="background: #362620; border: 1px solid rgba(77, 70, 58, 0.4)"
                >
                    <span style="color: #e3c76b">+</span>
                    <span
                        class="font-label text-xs tracking-widest uppercase"
                        style="color: #f9ddd3"
                        >{{ toastMsg }}</span
                    >
                </div>
            </div>
        </Teleport>

        <!-- Notify Toasts：只在 Step 1 選餐時顯示 -->
        <Teleport to="body">
            <div v-show="step === 1" class="notify-toast-stack">
                <TransitionGroup name="notify-toast">
                    <div
                        v-for="toast in notifyToasts"
                        :key="toast.key"
                        :class="[
                            'notify-toast-card',
                            !isLoggedIn
                                ? 'eligible'
                                : toast.type === 'near'
                                  ? 'near'
                                  : toast.type === 'eligible-notify'
                                    ? 'eligible'
                                    : toast.type === 'one-event-note'
                                      ? 'info'
                                      : toast.type === 'applied-event'
                                        ? 'applied'
                                        : 'eligible',
                        ]"
                    >
                        <button class="notify-toast-close" @click="dismissToast(toast.key)">
                            ✕
                        </button>
                        <div class="notify-toast-icon">
                            {{
                                toast.type === 'near'
                                    ? '🔥'
                                    : toast.type === 'one-event-note'
                                      ? 'ℹ️'
                                      : toast.type === 'applied-event'
                                        ? '🎉'
                                        : '🎁'
                            }}
                        </div>
                        <p class="font-body notify-toast-msg">
                            <template v-if="toast.type === 'near'"
                                >差 NT${{ toast.ev.minSpend - total }} 即可參加「{{
                                    toast.ev.title
                                }}」活動，享 {{ toast.ev.discountDescription }} 優惠！</template
                            >
                            <template v-else-if="toast.type === 'eligible-notify'">
                                <template v-if="!isLoggedIn">
                                    恭喜！金額已達活動門檻，<span
                                        @click="openAuthModal"
                                        style="
                                            color: #e3c76b;
                                            font-weight: 700;
                                            text-decoration: underline;
                                            cursor: pointer;
                                        "
                                        >登入會員</span
                                    >即可參加「{{ toast.ev.title }}」活動{{
                                        toast.ev.summary ? `(${toast.ev.summary})` : ''
                                    }}！
                                </template>
                                <template v-else>
                                    恭喜！金額已達門檻，可參加「{{ toast.ev.title }}」活動{{
                                        toast.ev.summary ? `(${toast.ev.summary})` : ''
                                    }}！（請洽現場服務人員）
                                </template>
                            </template>
                            <template v-else-if="toast.type === 'one-event-note'"
                                >每次用餐能參加一個活動，不得與其他優惠活動合併使用</template
                            >
                            <template v-else-if="toast.type === 'applied-event'"
                                >已參加「{{ toast.ev.title }}」活動 （滿 NT${{
                                    toast.ev.minSpend
                                }}
                                可享 {{ toast.ev.discountDescription }}）</template
                            >
                        </p>
                    </div>
                </TransitionGroup>
            </div>
        </Teleport>
    </div>
</template>

<script setup>
import { ref, reactive, computed, watch, onMounted, nextTick } from 'vue'
import { useOrderStore } from '@/stores/order'
import { useAuthStore } from '@/stores/auth'
import { RouterLink } from 'vue-router'
import apiFetch from '@/utils/apiFetch'
import DishDetailModal from '@/components/Order/DishDetailModal.vue'
import SetMealSelectModal from '@/components/Order/SetMealSelectModal.vue'

const store = useOrderStore()
const authStore = useAuthStore()

// ── 步驟 & 畫面狀態 ──────────────────────────────────
const step = ref(1)
const cartOpen = ref(false)

// ── 會員狀態（從全域 authStore）─────────────────────
const isLoggedIn = computed(() => authStore.isLoggedIn)
const currentMemberId = computed(() => authStore.member?.id ?? null)

// ── 菜單 ─────────────────────────────────────────────
const products = ref([])
const loading = ref(true)
const loadError = ref('')
const searchQuery = ref('')
const activeChip = ref('all')
const activeSidebarCat = ref('')
const curView = ref('list')

const imgErrors = reactive(new Set())
const imgFallback = reactive(new Map())

// ── 取餐時段 ─────────────────────────────────────────
const pickupTime = ref('')
const pickupError = ref(false)

const pickupTimeOptions = computed(() => {
    const slots = []
    const open = 11 * 60 // 開店 11:00
    const close = 21 * 60 // 關店 21:00
    const start = open + 30 // 開店後 30 分：11:30
    const end = close - 60 // 關店前 1 小時：20:00
    for (let m = start; m <= end; m += 30) {
        const h = Math.floor(m / 60)
        const mm = m % 60
        slots.push(`${String(h).padStart(2, '0')}:${String(mm).padStart(2, '0')}`)
    }
    return slots
})

// ── 顧客資料 ─────────────────────────────────────────
const customerName = ref('')
const customerPhone = ref('')
const needUtensils = ref(false)
const nameError = ref(false)
const phoneError = ref(false)

// 登入後自動帶入會員姓名
watch(
    () => authStore.member?.name,
    (name) => {
        if (name) customerName.value = name
    },
    { immediate: true }
)
// 登入後自動帶入會員電話  ← 新增
watch(
    () => authStore.member?.phone, // ← key 名稱依你 authStore 實際欄位調整
    (phone) => {
        if (phone) customerPhone.value = phone
    },
    { immediate: true }
)

// ── 訂單完成後儲存（成功頁顯示用）─────────────────
const confirmedPickupTime = ref('')
const confirmedName = ref('')
const confirmedPhone = ref('')
const confirmedNeedUtensils = ref(false)
const confirmedTotal = ref(0)
const confirmedItems = ref([]) // 快照購物車（store.clearOrder 前存入）
const confirmedNote = ref('')
const confirmedGift = ref(null)
const confirmedEventTitle = ref('')
const confirmedEventDesc = ref('')
const confirmedEventDiscountType = ref('') // 'FixedAmount' | 'Percent' | 'Gift' | ''
const confirmedAutoEventDiscount = ref(0)
const confirmedCouponCode = ref('')
const confirmedCouponDiscount = ref(0)
const confirmedCouponMsg = ref('')
const confirmedSubtotal = ref(0) // 餐點原價合計（未含折扣）
const orderProgress = ref(1) // 動態進度條：1=接收 2=製作中 3=完成

// ── Modal 狀態 ───────────────────────────────────────
const activeDetail = ref(null)
const editingLineId = ref(null)
const activeMeal = ref(null)
const editingMealLineId = ref(null)
const successModalOpen = ref(false)
const orderNumber = ref('')
const submitting = ref(false)

// ── 優惠券 ──────────────────────────────────────────
const couponCode = ref('')
const couponMsg = ref('')
const couponOk = ref(false)
const couponId = ref(null)
const couponDiscount = ref(0)

// 優惠券 × 活動衝突警告 Modal
const couponEventWarnModal = ref(false)
const pendingCouponData = ref(null) // { discount, couponId, message }

// 已領優惠券下拉選單
const myCoupons = ref([])
const selectedCouponCode = ref('')

// 只顯示未使用且未過期的券
const availableCoupons = computed(() => myCoupons.value.filter((c) => !c.isUsed && !c.isExpired))

async function loadMyCoupons() {
    try {
        const res = await apiFetch('/Coupons/My')
        if (res.ok) myCoupons.value = await res.json()
    } catch {}
}

// 下拉選單選擇優惠券時自動套用；選到特殊項目時導頁
async function onCouponSelect() {
    if (selectedCouponCode.value === '__coupons__') {
        selectedCouponCode.value = ''
        window.location.href = '/coupons'
        return
    }
    if (!selectedCouponCode.value) {
        resetCoupon()
        return
    }
    couponCode.value = selectedCouponCode.value
    await applyCoupon()
}

function resetCoupon() {
    couponCode.value = ''
    couponMsg.value = ''
    couponOk.value = false
    couponId.value = null
    couponDiscount.value = 0
    selectedCouponCode.value = ''
}

// ── 活動 ─────────────────────────────────────────────
const autoEvents = ref([])
const notifyEvents = ref([])
const nearAutoEvents = ref([])
const giftCartItem = ref(null)
const notifyToasts = ref([])
let _toastKeySeq = 0
const _nearToastKeys = new Map()
const _eligibleNotifyKeys = new Map()
const _guestAutoKeys = new Map()
let _oneEventNoteKey = null
let _lastBestEventId = null
let _appliedEventToastTimer = null

// ── 收藏 / 歷史訂單 ──────────────────────────────────
const favoriteProducts = ref([])
const orderHistory = ref([])

async function loadFavorites() {
    try {
        const res = await apiFetch('/Orders/Favorites')
        if (res.ok) favoriteProducts.value = await res.json()
    } catch {}
}

async function loadOrderHistory() {
    try {
        const res = await apiFetch('/Orders/MemberOrderHistory')
        if (res.ok) orderHistory.value = await res.json()
    } catch {}
}

// 開啟全域登入 Modal（Bootstrap #authModal，與 Navbar 共用）
function openAuthModal() {
    const modalEl = document.querySelector('#authModal')
    if (modalEl) {
        import('bootstrap').then(({ Modal }) => {
            Modal.getOrCreateInstance(modalEl).show()
        })
    }
}

function reorder(order) {
    let hasSetMeal = false
    order.items.forEach((item) => {
        const matched = products.value.find((p) => p.productName === item.productName)
        if (!matched) return

        if (item.isSetMeal && matched.isSetMeal && matched.setMealId) {
            // 套餐：加入購物車並帶入基本 setMealData（id 必須有值，讓點擊後可開啟編輯選單）
            // fixedItems / selectedOptions 留空，提示使用者點擊確認內容
            hasSetMeal = true
            for (let i = 0; i < item.qty; i++) {
                store.addSetMeal(matched.productId, matched.unitPrice, item.note || '', {
                    id: matched.setMealId,
                    name: matched.productName,
                    fixedItems: [],
                    selectedOptions: [],
                })
            }
        } else {
            for (let i = 0; i < item.qty; i++) {
                store.addItem(matched.productId, item.note || '')
            }
        }
    })
    activeSidebarCat.value = '全部'
    showToast(hasSetMeal ? '已加入購物車（套餐請點擊確認內容）' : '已加入購物車')
}

// ── Toast ────────────────────────────────────────────
const toastVisible = ref(false)
const toastMsg = ref('')
let toastTimer = null

function showToast(msg) {
    toastMsg.value = msg
    toastVisible.value = true
    clearTimeout(toastTimer)
    toastTimer = setTimeout(() => {
        toastVisible.value = false
    }, 2500)
}

function dismissToast(key) {
    notifyToasts.value = notifyToasts.value.filter((t) => t.key !== key)
}
function pushToast(ev, opts = {}) {
    const key = ++_toastKeySeq
    notifyToasts.value.push({ key, ev: { ...ev }, ...opts })
    return key
}

// ── Computed ─────────────────────────────────────────

const cartItemsWithDetails = computed(() =>
    store.lines
        .map((line) => {
            if (line.isSetMeal) {
                return {
                    lineId: line.lineId,
                    productId: line.productId,
                    productName: line.setMealData?.name ?? '套餐',
                    unitPrice: line.unitPrice ?? 0,
                    qty: line.qty,
                    note: line.note,
                    isSetMeal: true,
                    setMealData: line.setMealData,
                }
            }
            const d = products.value.find((p) => p.productId === line.productId)
            return d ? { ...d, lineId: line.lineId, qty: line.qty, note: line.note } : null
        })
        .filter(Boolean)
)

const total = computed(() =>
    cartItemsWithDetails.value.reduce((s, i) => s + i.unitPrice * i.qty, 0)
)

const bestAutoEvent = computed(() => {
    if (!autoEvents.value.length) return null
    return [...autoEvents.value].sort((a, b) =>
        b.minSpend !== a.minSpend
            ? b.minSpend - a.minSpend
            : (b.calculatedDiscount ?? 0) - (a.calculatedDiscount ?? 0)
    )[0]
})

const autoEventDiscount = computed(() =>
    isLoggedIn.value && bestAutoEvent.value ? (bestAutoEvent.value.calculatedDiscount ?? 0) : 0
)

const finalTotal = computed(() =>
    Math.max(0, total.value - couponDiscount.value - autoEventDiscount.value)
)

// ── 分類 Computed ─────────────────────────────────────
const CATEGORY_ORDER = ['套餐', '主餐', '湯品', '甜點', '附餐', '飲料']

const sidebarCategories = computed(() => {
    const specials = [
        {
            key: '今日推薦',
            label: '今日推薦',
            count: products.value.filter((p) => p.isRecommended).length,
        },
        {
            key: '主廚特選',
            label: '主廚特選',
            count: products.value.filter((p) => p.isPopular).length,
        },
    ].filter((s) => s.count > 0)

    if (isLoggedIn.value) {
        specials.push(
            { key: '我的收藏', label: '我的收藏', count: favoriteProducts.value.length },
            { key: '歷史訂單', label: '歷史訂單', count: orderHistory.value.length }
        )
    }

    const map = new Map()
    products.value.forEach((p) => {
        if (!p.categoryName) return
        if (!map.has(p.categoryName))
            map.set(p.categoryName, { key: p.categoryName, label: p.categoryName, count: 0 })
        map.get(p.categoryName).count++
    })
    const cats = Array.from(map.values()).sort((a, b) => {
        const ai = CATEGORY_ORDER.indexOf(a.key),
            bi = CATEGORY_ORDER.indexOf(b.key)
        return (ai < 0 ? 99 : ai) - (bi < 0 ? 99 : bi)
    })

    return [...specials, { key: '全部', label: '全部', count: products.value.length }, ...cats]
})

const filteredProducts = computed(() => {
    let r = products.value
    if (searchQuery.value) {
        const t = searchQuery.value.toLowerCase()
        r = r.filter((p) => p.productName?.toLowerCase().includes(t))
    }
    if (activeChip.value === 'veg') r = r.filter((p) => p.isVegetarian)
    if (activeChip.value === 'spicy') r = r.filter((p) => p.spicyLevel > 0)
    return r
})

const displaySections = computed(() => {
    if (activeSidebarCat.value === '我的收藏') {
        return favoriteProducts.value.length
            ? [{ key: '我的收藏', label: '我的收藏', dishes: favoriteProducts.value }]
            : []
    }
    if (activeSidebarCat.value === '歷史訂單') {
        return []
    }
    if (searchQuery.value.trim()) {
        const dishes = filteredProducts.value
        return dishes.length ? [{ key: 'search', label: `搜尋：${searchQuery.value}`, dishes }] : []
    }
    if (activeSidebarCat.value === '今日推薦') {
        const dishes = filteredProducts.value.filter((p) => p.isRecommended)
        return dishes.length ? [{ key: '今日推薦', label: '今日推薦', dishes }] : []
    }
    if (activeSidebarCat.value === '主廚特選') {
        const dishes = filteredProducts.value.filter((p) => p.isPopular)
        return dishes.length ? [{ key: '主廚特選', label: '主廚特選', dishes }] : []
    }
    if (activeSidebarCat.value === '全部') {
        return CATEGORY_ORDER.map((cat) => ({
            key: cat,
            label: cat,
            dishes: filteredProducts.value.filter((p) => p.categoryName === cat),
        })).filter((s) => s.dishes.length > 0)
    }
    const dishes = filteredProducts.value.filter((p) => p.categoryName === activeSidebarCat.value)
    return dishes.length
        ? [{ key: activeSidebarCat.value, label: activeSidebarCat.value, dishes }]
        : []
})

// ── 活動 API ─────────────────────────────────────────
async function fetchActiveEvents() {
    try {
        const res = await apiFetch(`/Orders/ActiveEvents?amount=${total.value}`)
        if (!res.ok) return
        const data = await res.json()
        autoEvents.value = data.autoEvents ?? []
        notifyEvents.value = data.notifyEvents ?? []
        nearAutoEvents.value = data.nearAutoEvents ?? []

        const autoIdSet = new Set(autoEvents.value.map((e) => e.id))
        const nearAutoIdSet = new Set(nearAutoEvents.value.map((e) => e.id))
        const hasBest = !!bestAutoEvent.value

        // ── 清除失效的訪客 autoEvent toast（已登入 or 事件已失效）──
        for (const [id, key] of _guestAutoKeys) {
            if (!autoIdSet.has(id) || isLoggedIn.value) {
                dismissToast(key)
                _guestAutoKeys.delete(id)
            }
        }

        // ── 清除失效的差額 toast（事件不再差額≤100）──
        for (const [id, key] of _nearToastKeys) {
            const stillNearAuto = nearAutoIdSet.has(id)
            const stillNearNotify = notifyEvents.value.some(
                (e) => e.id === id && !e.isEligible && e.minSpend - total.value <= 100
            )
            if (!stillNearAuto && !stillNearNotify) {
                dismissToast(key)
                _nearToastKeys.delete(id)
            }
        }

        // ── 清除失效的 eligible-notify toast ──
        for (const [id, key] of _eligibleNotifyKeys) {
            const stillEligible = notifyEvents.value.some((e) => e.id === id && e.isEligible)
            if (!stillEligible || hasBest) {
                dismissToast(key)
                _eligibleNotifyKeys.delete(id)
            }
        }

        if (!hasBest && _oneEventNoteKey !== null) {
            dismissToast(_oneEventNoteKey)
            _oneEventNoteKey = null
        }

        // ── 0. 訪客：autoEvents 達門檻 → eligible-notify toast（含登入連結）──
        if (!isLoggedIn.value) {
            autoEvents.value.forEach((ev) => {
                if (!_guestAutoKeys.has(ev.id)) {
                    const key = pushToast(ev, { persistent: true, type: 'eligible-notify' })
                    _guestAutoKeys.set(ev.id, key)
                }
            })
        }

        // ── 1. nearAutoEvents（差額≤100）→ 差額 toast ──
        nearAutoEvents.value.forEach((ev) => {
            if (!_nearToastKeys.has(ev.id)) {
                _nearToastKeys.set(ev.id, pushToast(ev, { persistent: true, type: 'near' }))
            }
        })

        // ── 2. notifyEvents（IsAutoDiscount=0）──
        notifyEvents.value.forEach((ev) => {
            const gap = ev.minSpend - total.value
            if (ev.isEligible) {
                const nearKey = _nearToastKeys.get(ev.id)
                if (nearKey !== undefined) {
                    dismissToast(nearKey)
                    _nearToastKeys.delete(ev.id)
                }
                if ((!hasBest || !isLoggedIn.value) && !_eligibleNotifyKeys.has(ev.id)) {
                    _eligibleNotifyKeys.set(
                        ev.id,
                        pushToast(ev, { persistent: true, type: 'eligible-notify' })
                    )
                }
            } else if (gap <= 100 && !_nearToastKeys.has(ev.id)) {
                _nearToastKeys.set(ev.id, pushToast(ev, { persistent: true, type: 'near' }))
            }
        })

        // ── 3. bestAutoEvent 存在且有其他符合活動 → 「一個活動限制」提示 ──
        const hasOtherEligible =
            notifyEvents.value.some((e) => e.isEligible) || autoEvents.value.length > 1
        if (hasBest && hasOtherEligible && _oneEventNoteKey === null) {
            _oneEventNoteKey = pushToast(
                { id: -1, title: '', discountDescription: '' },
                { persistent: true, type: 'one-event-note' }
            )
        }
    } catch {
        /* 靜默 */
    }
}

watch(total, (val) => {
    if (val > 0) {
        fetchActiveEvents()
    } else {
        autoEvents.value = []
        notifyEvents.value = []
        nearAutoEvents.value = []
        notifyToasts.value = []
        _nearToastKeys.clear()
        _eligibleNotifyKeys.clear()
        _guestAutoKeys.clear()
        _oneEventNoteKey = null
        giftCartItem.value = null
        _lastBestEventId = null
        clearTimeout(_appliedEventToastTimer)
    }
})

watch(bestAutoEvent, (newEv) => {
    giftCartItem.value = null
    if (isLoggedIn.value && newEv?.discountType === 'Gift' && newEv.rewardDishId) {
        const dish = products.value.find((p) => p.productId === newEv.rewardDishId)
        if (dish)
            giftCartItem.value = {
                productId: dish.productId,
                productName: dish.productName,
                qty: 1,
            }
    }
    const newId = newEv?.id ?? null
    if (newId !== _lastBestEventId) {
        _lastBestEventId = newId
        if (newEv && isLoggedIn.value) {
            clearTimeout(_appliedEventToastTimer)
            const key = pushToast(newEv, { persistent: false, type: 'applied-event' })
            _appliedEventToastTimer = setTimeout(() => dismissToast(key), 2000)
        }
    }
})

// ── 優惠券 API ───────────────────────────────────────
async function applyCoupon() {
    if (!couponCode.value.trim()) return
    couponOk.value = false
    couponId.value = null
    couponDiscount.value = 0
    couponMsg.value = ''
    try {
        const res = await apiFetch('/Orders/ValidateCoupon', {
            method: 'POST',
            body: JSON.stringify({
                code: couponCode.value.trim().toUpperCase(),
                orderAmount: total.value,
            }),
        })
        const data = await res.json()
        if (data.isValid) {
            const discount = data.discount ?? 0
            // 已套用活動且使用優惠券後金額低於活動門檻 → 先警告
            if (
                isLoggedIn.value &&
                bestAutoEvent.value &&
                total.value - discount < bestAutoEvent.value.minSpend
            ) {
                pendingCouponData.value = {
                    discount,
                    couponId: data.couponId,
                    message: data.message || `折抵 NT$ ${discount}`,
                }
                couponEventWarnModal.value = true
            } else {
                couponMsg.value = data.message || `折抵 NT$ ${discount}`
                couponOk.value = true
                couponId.value = data.couponId
                couponDiscount.value = discount
            }
        } else {
            couponMsg.value = data.message || '優惠券無效或不符條件'
        }
    } catch {
        couponMsg.value = '驗證失敗，請稍後再試'
    }
}

// 使用者確認「仍要用優惠券」
function confirmCouponDespiteEvent() {
    if (!pendingCouponData.value) return
    const { discount, couponId: cid, message } = pendingCouponData.value
    couponMsg.value = message
    couponOk.value = true
    couponId.value = cid
    couponDiscount.value = discount
    pendingCouponData.value = null
    couponEventWarnModal.value = false
}

// 使用者取消（不套用優惠券）
function cancelCouponWarn() {
    pendingCouponData.value = null
    couponEventWarnModal.value = false
}

// ── 步驟流程 ─────────────────────────────────────────
function goToStep2() {
    if (store.totalItems === 0) return
    if (!pickupTime.value) {
        pickupError.value = true
        showToast('請先選擇取餐時段')
        return
    }
    step.value = 2
    window.scrollTo({ top: 0, behavior: 'smooth' })
}

function goToStep3() {
    let valid = true
    if (!customerName.value.trim()) {
        nameError.value = true
        valid = false
    }
    if (!customerPhone.value.trim() || !/^[\d\-\+\s]{8,}$/.test(customerPhone.value.trim())) {
        phoneError.value = true
        valid = false
    }
    if (!valid) return
    step.value = 3
    window.scrollTo({ top: 0, behavior: 'smooth' })
}

// ── 菜單互動 ─────────────────────────────────────────
function toggleChip(c) {
    activeChip.value = activeChip.value === c ? 'all' : c
}

function scrollToSection(cat) {
    activeSidebarCat.value = cat
    searchQuery.value = ''
    // menu 本身是捲動容器，捲回頂部
    document.getElementById('outMenuMain')?.scrollTo({ top: 0, behavior: 'smooth' })
}

function handleRemove(dish) {
    store.removeItem(dish.productId)
}

async function openDetail(dish) {
    if (dish.isSetMeal && dish.setMealId) {
        editingMealLineId.value = null
        try {
            const res = await apiFetch(`/SetMeals/${dish.setMealId}`)
            if (res.ok) activeMeal.value = await res.json()
        } catch {
            /* 靜默 */
        }
        return
    }
    editingLineId.value = null
    activeDetail.value = dish
}

async function openCartItemEdit(item) {
    if (item.isSetMeal) {
        editingMealLineId.value = item.lineId
        const smId = item.setMealData?.id
        if (!smId) return
        try {
            const res = await apiFetch(`/SetMeals/${smId}`)
            if (res.ok) activeMeal.value = await res.json()
        } catch {
            /* 靜默 */
        }
        return
    }
    editingLineId.value = item.lineId
    activeDetail.value = products.value.find((p) => p.productId === item.productId) ?? item
}

function onDetailConfirm(dish, qty, note) {
    if (editingLineId.value !== null) {
        store.removeLineItem(editingLineId.value)
        for (let i = 0; i < qty; i++) store.addItem(dish.productId, note || '')
        showToast(`「${dish.productName}」已更新`)
        editingLineId.value = null
    } else {
        for (let i = 0; i < qty; i++) store.addItem(dish.productId, note || '')
        showToast(`「${dish.productName}」× ${qty} 已加入訂單`)
    }
    activeDetail.value = null
}

function buildInitialSel(lineId) {
    const line = store.lines.find((l) => l.lineId === lineId)
    if (!line?.setMealData?.selectedOptions) return {}
    const sel = {}
    for (const opt of line.setMealData.selectedOptions) {
        sel[opt.groupNo ?? 0] = sel[opt.groupNo ?? 0] ?? {}
        sel[opt.groupNo ?? 0][opt.dishId] = opt.qty
    }
    return sel
}

function onSetMealConfirm(meal, qty, note, selectedOptions) {
    const setMealData = {
        id: meal.id,
        name: meal.setMealName,
        fixedItems: (meal.items ?? []).filter((i) => !i.isOptional),
        selectedOptions,
    }
    if (editingMealLineId.value !== null) {
        store.updateSetMealLine(editingMealLineId.value, meal.setPrice, note, setMealData)
        showToast(`「${meal.setMealName}」已更新`)
        editingMealLineId.value = null
    } else {
        for (let i = 0; i < qty; i++) {
            store.addSetMeal(
                products.value.find((p) => p.setMealId === meal.id && p.isSetMeal)?.productId ?? 0,
                meal.setPrice,
                note,
                setMealData
            )
        }
        showToast(`「${meal.setMealName}」× ${qty} 已加入訂單`)
    }
    activeMeal.value = null
}

// ── 送出訂單 ─────────────────────────────────────────
async function submitOrder() {
    if (store.totalItems === 0 || submitting.value) return
    submitting.value = true
    try {
        const itemsPayload = (() => {
            const result = []
            for (const i of cartItemsWithDetails.value) {
                if (i.isSetMeal) {
                    const parentIdx = result.length
                    result.push({
                        productId: i.productId,
                        productName: i.productName,
                        qty: i.qty,
                        unitPrice: i.unitPrice,
                        isSetMeal: true,
                        note: i.note || null,
                        parentIndex: null,
                    })
                    for (const f of i.setMealData?.fixedItems ?? []) {
                        for (let q = 0; q < (f.quantity || 1); q++) {
                            result.push({
                                productId: f.dishId ?? 0,
                                productName: f.dishName,
                                qty: 1,
                                unitPrice: 0,
                                isSetMeal: true,
                                parentIndex: parentIdx,
                            })
                        }
                    }
                    for (const s of i.setMealData?.selectedOptions ?? []) {
                        for (let q = 0; q < (s.qty || 1); q++) {
                            result.push({
                                productId: s.dishId ?? 0,
                                productName: s.dishName,
                                qty: 1,
                                unitPrice: 0,
                                isSetMeal: true,
                                parentIndex: parentIdx,
                            })
                        }
                    }
                } else {
                    result.push({
                        productId: i.productId,
                        productName: i.productName,
                        qty: i.qty,
                        unitPrice: i.unitPrice,
                        isSetMeal: false,
                        note: i.note || null,
                        parentIndex: null,
                    })
                }
            }
            if (isLoggedIn.value && giftCartItem.value) {
                result.push({
                    productId: giftCartItem.value.productId,
                    productName: giftCartItem.value.productName,
                    qty: 1,
                    unitPrice: 0,
                    isSetMeal: false,
                    note: '贈品',
                    parentIndex: null,
                })
            }
            return result
        })()

        const body = {
            tableId: 0,
            inOrOut: false,
            peopleNum: 1,
            isAddOrder: false,
            payMethod: 'Cash',
            note: [
                `取餐時間：${pickupTime.value}`,
                `取餐人：${customerName.value.trim()}`,
                `電話：${customerPhone.value.trim()}`,
                `餐具：${needUtensils.value ? '需要' : '不需要'}`,
                store.specialRequest ? `備註：${store.specialRequest}` : '',
            ]
                .filter(Boolean)
                .join('\n'),
            memberId: currentMemberId.value,
            couponId: couponId.value,
            discountAmount: (couponOk.value ? couponDiscount.value : 0) + autoEventDiscount.value,
            items: itemsPayload,
        }

        const res = await apiFetch('/Orders/CreatePreOrder', {
            method: 'POST',
            body: JSON.stringify(body),
        })
        if (!res.ok) throw new Error(`HTTP ${res.status}`)
        const data = await res.json()

        orderNumber.value = data.orderNumber
        confirmedPickupTime.value = pickupTime.value
        confirmedName.value = customerName.value
        confirmedPhone.value = customerPhone.value
        confirmedNeedUtensils.value = needUtensils.value
        confirmedTotal.value = finalTotal.value
        confirmedNote.value = store.specialRequest || ''
        // 快照購物車（store.clearOrder 前存入，否則資料消失）
        confirmedItems.value = cartItemsWithDetails.value.map((item) => ({
            ...item,
            setMealData: item.setMealData ? JSON.parse(JSON.stringify(item.setMealData)) : null,
        }))
        confirmedGift.value = giftCartItem.value ? { ...giftCartItem.value } : null

        // 快照活動 / 優惠券（store.clearOrder & resetCoupon 前存入）
        confirmedSubtotal.value = total.value
        if (isLoggedIn.value && bestAutoEvent.value) {
            confirmedEventTitle.value = bestAutoEvent.value.title
            confirmedEventDesc.value = bestAutoEvent.value.discountDescription
            confirmedEventDiscountType.value = bestAutoEvent.value.discountType ?? ''
            confirmedAutoEventDiscount.value = autoEventDiscount.value
        } else {
            confirmedEventTitle.value = ''
            confirmedEventDesc.value = ''
            confirmedEventDiscountType.value = ''
            confirmedAutoEventDiscount.value = 0
        }
        if (couponOk.value && couponDiscount.value > 0) {
            confirmedCouponCode.value = couponCode.value
            confirmedCouponDiscount.value = couponDiscount.value
            confirmedCouponMsg.value = couponMsg.value
        } else {
            confirmedCouponCode.value = ''
            confirmedCouponDiscount.value = 0
            confirmedCouponMsg.value = ''
        }

        store.clearOrder()
        resetCoupon()

        // 進入成功頁並捲回頂部
        orderProgress.value = 1
        step.value = 4
        window.scrollTo({ top: 0, behavior: 'smooth' })

        // 動態進度：1.5 秒後推進到「餐點製作中」
        setTimeout(() => {
            orderProgress.value = 2
        }, 1500)
    } catch (err) {
        console.error(err)
        showToast('送出失敗，請稍後再試')
    } finally {
        submitting.value = false
    }
}

function onSuccessClose() {
    step.value = 1
    pickupTime.value = ''
    // 優先從會員資料還原，未登入才清空（避免下次點餐需重填）
    customerName.value = authStore.member?.name || ''
    customerPhone.value = authStore.member?.phone || ''
    needUtensils.value = false
    confirmedItems.value = []
    confirmedGift.value = null
    confirmedEventTitle.value = ''
    confirmedEventDesc.value = ''
    confirmedEventDiscountType.value = ''
    confirmedAutoEventDiscount.value = 0
    confirmedCouponCode.value = ''
    confirmedCouponDiscount.value = 0
    confirmedCouponMsg.value = ''
    confirmedSubtotal.value = 0
    confirmedNeedUtensils.value = false
    orderProgress.value = 1
    window.scrollTo({ top: 0 })
}

function clearAll() {
    store.clearOrder()
    resetCoupon()
}

function handleImgError(dish) {
    const current = imgFallback.get(dish.productId) || resolveImage(dish.imageUrl)
    if (/\.jpg$/i.test(current)) {
        imgFallback.set(dish.productId, current.replace(/\.jpg$/i, '.png'))
    } else {
        imgErrors.add(dish.productId)
    }
}

function resolveImage(url) {
    if (!url) return ''
    let path = url.replace(/\\/g, '/').replace(/^~\//, '')
    const match = /\/wwwroot\/(.*)/i.exec('/' + path)
    if (match?.[1]) path = match[1]
    return `/${path.replace(/^\//, '')}`
}

watch(isLoggedIn, (loggedIn) => {
    if (loggedIn) {
        loadFavorites()
        loadOrderHistory()
        loadMyCoupons()
        // 登入後重新 fetch：清掉訪客 toast、改以會員身分計算活動
        if (total.value > 0) fetchActiveEvents()
    } else {
        favoriteProducts.value = []
        orderHistory.value = []
        myCoupons.value = []
        resetCoupon()
    }
})

onMounted(async () => {
    // 路由守衛已 await checkAuth()，此時 isLoggedIn 已確定
    if (isLoggedIn.value) {
        loadFavorites()
        loadOrderHistory()
        loadMyCoupons()
    }

    // 量 navbar + step-banner + 手機分類列 真實高度，設成 CSS 變數
    const navbar = document.querySelector('nav.navbar-eat') ?? document.querySelector('.navbar')
    const banner = document.querySelector('.step-banner')
    if (navbar) {
        const navH = Math.ceil(navbar.getBoundingClientRect().height)
        document.documentElement.style.setProperty('--navbar-h', `${navH}px`)
        // step-banner 在 navbar 正下方，量好後算出 navbar + step-banner 總高
        await nextTick()
        const banH = banner ? Math.ceil(banner.getBoundingClientRect().height) : 73
        document.documentElement.style.setProperty('--top-fixed', `${navH + banH}px`)
        // 手機版分類列（.mobile-cat-bar）固定在 step-banner 正下方
        // 量測其高度供 toolbar sticky top 計算使用
        await nextTick()
        const catBar = document.querySelector('.mobile-cat-bar')
        const catBarH = catBar ? Math.ceil(catBar.getBoundingClientRect().height) : 40
        document.documentElement.style.setProperty('--cat-bar-h', `${catBarH}px`)
    }

    try {
        const res = await apiFetch('/Orders/MenuItems')
        if (!res.ok) throw new Error()
        products.value = await res.json()
        activeSidebarCat.value = sidebarCategories.value[0]?.key ?? ''
    } catch {
        loadError.value = '菜單載入失敗，請稍後再試。'
    } finally {
        loading.value = false
    }

    // 頁面載入時若購物車（Pinia 持久化）已有品項，主動觸發一次活動計算
    // watch(total) 只在值「改變」時才跑，初始值不觸發，故需在此補呼叫
    if (total.value > 0) fetchActiveEvents()
})
</script>

<style>
/* ── Notify Toast（右下角，Teleport to body） ── */
.notify-toast-stack {
    position: fixed;
    bottom: 1.5rem;
    right: 1.25rem;
    z-index: 8500;
    display: flex;
    flex-direction: column-reverse;
    gap: 0.6rem;
    max-width: 300px;
    pointer-events: none;
}
.notify-toast-card {
    position: relative;
    border-radius: 0.6rem;
    padding: 0.75rem 2.2rem 0.75rem 1rem;
    display: flex;
    align-items: flex-start;
    gap: 0.55rem;
    box-shadow: 0 6px 24px rgba(0, 0, 0, 0.45);
    pointer-events: all;
}
.notify-toast-card.eligible {
    background: #1e2d1a;
    border: 1px solid rgba(163, 217, 119, 0.45);
    border-left: 3px solid #a3d977;
}
.notify-toast-card.near {
    background: #2a1e10;
    border: 1px solid rgba(255, 160, 80, 0.45);
    border-left: 3px solid #ff9f4a;
}
.notify-toast-card.info {
    background: #141c25;
    border: 1px solid rgba(130, 170, 220, 0.35);
    border-left: 3px solid #82aadc;
}
.notify-toast-card.applied {
    background: #221a0e;
    border: 1px solid rgba(227, 199, 107, 0.45);
    border-left: 3px solid #e3c76b;
}
.notify-toast-icon {
    font-size: 1.1rem;
    flex-shrink: 0;
    margin-top: 0.05rem;
}
.notify-toast-msg {
    font-size: 0.78rem;
    line-height: 1.5;
    margin: 0;
    color: #f9ddd3;
}
.notify-toast-card.eligible .notify-toast-msg {
    color: #c4eda0;
}
.notify-toast-card.near .notify-toast-msg {
    color: #ffd0a0;
}
.notify-toast-card.info .notify-toast-msg {
    color: rgba(180, 210, 240, 0.8);
}
.notify-toast-card.applied .notify-toast-msg {
    color: #f5dfa0;
}
.notify-toast-close {
    position: absolute;
    top: 0.4rem;
    right: 0.5rem;
    background: none;
    border: none;
    color: rgba(208, 197, 181, 0.35);
    font-size: 0.7rem;
    cursor: pointer;
    padding: 0.15rem 0.3rem;
    transition: color 0.2s;
    line-height: 1;
}
.notify-toast-close:hover {
    color: rgba(208, 197, 181, 0.8);
}
.notify-toast-enter-active {
    transition:
        opacity 0.3s,
        transform 0.3s;
}
.notify-toast-leave-active {
    transition:
        opacity 0.25s,
        transform 0.25s;
    position: absolute;
}
.notify-toast-enter-from {
    opacity: 0;
    transform: translateX(40px);
}
.notify-toast-leave-to {
    opacity: 0;
    transform: translateX(40px);
}

/* ── simple toast ── */
/* ── 優惠券 × 活動衝突警告 Modal ── */
.cew-overlay {
    position: fixed;
    inset: 0;
    background: rgba(0, 0, 0, 0.65);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 9500;
}
.cew-box {
    background: #2a1a12;
    border: 1px solid rgba(227, 199, 107, 0.3);
    border-radius: 0.75rem;
    padding: 2rem 1.75rem 1.5rem;
    max-width: 360px;
    width: 90%;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 0.75rem;
    box-shadow: 0 8px 32px rgba(0, 0, 0, 0.5);
}
.cew-icon {
    font-size: 2rem;
    line-height: 1;
}
.cew-title {
    color: #e3c76b;
    font-size: 1.15rem;
    margin: 0;
}
.cew-body {
    color: rgba(208, 197, 181, 0.85);
    font-size: 0.9rem;
    line-height: 1.65;
    text-align: center;
    margin: 0;
}
.cew-highlight {
    color: #e3c76b;
    font-weight: 700;
}
.cew-sub {
    color: rgba(208, 197, 181, 0.55);
    font-size: 0.82rem;
    margin: 0;
}
.cew-btns {
    display: flex;
    gap: 0.75rem;
    width: 100%;
    margin-top: 0.25rem;
}
.cew-btn-cancel {
    flex: 1;
    padding: 0.65rem;
    border: 1px solid rgba(77, 70, 58, 0.5);
    border-radius: 0.35rem;
    background: transparent;
    color: rgba(208, 197, 181, 0.6);
    font-size: 0.9rem;
    cursor: pointer;
    transition:
        border-color 0.2s,
        color 0.2s;
}
.cew-btn-cancel:hover {
    border-color: rgba(208, 197, 181, 0.4);
    color: rgba(208, 197, 181, 0.9);
}
.cew-btn-confirm {
    flex: 1;
    padding: 0.65rem;
    border: none;
    border-radius: 0.35rem;
    background: linear-gradient(135deg, #e3c76b, #c6ab53);
    color: #3b2f00;
    font-size: 0.9rem;
    font-weight: 700;
    cursor: pointer;
    transition: opacity 0.2s;
}
.cew-btn-confirm:hover {
    opacity: 0.88;
}
.cew-fade-enter-active,
.cew-fade-leave-active {
    transition: opacity 0.2s;
}
.cew-fade-enter-from,
.cew-fade-leave-to {
    opacity: 0;
}

.simple-toast {
    position: fixed;
    bottom: 4.5rem;
    left: 50%;
    transform: translateX(-50%);
    z-index: 9900;
    transition:
        opacity 0.3s,
        transform 0.3s;
    border-radius: 0.375rem;
    overflow: hidden;
}

/* ════ 步驟 Banner 右側區塊 ════ */
.banner-right {
    display: flex;
    align-items: center;
    gap: 0.75rem;
    flex-shrink: 0;
}
/* 訂單查詢觸發連結 */
.lookup-trigger-btn {
    display: inline-flex;
    align-items: center;
    gap: 0.35rem;
    padding: 0.32rem 0.75rem;
    background: transparent;
    border: 1px solid rgba(227, 199, 107, 0.35);
    border-radius: 999px;
    color: rgba(227, 199, 107, 0.8);
    font-size: 0.78rem;
    letter-spacing: 0.1em;
    text-decoration: none;
    cursor: pointer;
    transition:
        background 0.2s,
        color 0.2s,
        border-color 0.2s;
    white-space: nowrap;
}
.lookup-trigger-btn:hover {
    background: rgba(227, 199, 107, 0.1);
    color: #e3c76b;
    border-color: rgba(227, 199, 107, 0.65);
}
</style>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Noto+Serif+TC:wght@400;700&family=Newsreader:ital,wght@0,400;0,600;1,400&family=Work+Sans:wght@300;400&display=swap');

/* ════ 全域包裝 ════ */
.out-wrap {
    background: #1e100b;
    color: #f9ddd3;
    font-family: 'Newsreader', serif;
    min-height: 100vh;
    padding-top: calc(27px); /* navbar + step-banner */
}

/* ════ Step Banner ════ */
.step-banner {
    background: #180b06;
    border-bottom: 1px solid rgba(77, 70, 58, 0.3);
    padding: 0.5rem 2.5rem;
    display: flex;
    align-items: center;
    justify-content: space-between;
    flex-wrap: wrap;
    gap: 0.75rem;
    position: fixed;
    top: var(--navbar-h, 80px);
    left: 0;
    right: 0;
    z-index: 60;
}
.step-items {
    display: flex;
    align-items: center;
    gap: 0.75rem;
    flex: 1;
    width: 100%;
}
.step-dot {
    width: 40px;
    height: 40px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    font-family: 'Work Sans', sans-serif;
    font-size: 0.95rem;
    font-weight: 600;
    border: 1.5px solid rgba(77, 70, 58, 0.6);
    color: rgba(208, 197, 181, 0.4);
    background: #2b1c16;
    transition: all 0.35s;
    flex-shrink: 0;
}
.step-dot.active {
    border-color: #e3c76b;
    color: #e3c76b;
    background: rgba(227, 199, 107, 0.1);
}
.step-dot.done {
    border-color: #c6ab53;
    background: #c6ab53;
    color: #3b2f00;
}
.step-line {
    flex: 1;
    height: 1px;
    background: rgba(77, 70, 58, 0.45);
}
.step-lbl {
    font-family: 'Work Sans', sans-serif;
    font-size: 0.8rem;
    letter-spacing: 0.12em;
    text-transform: uppercase;
    color: rgba(208, 197, 181, 0.4);
    white-space: nowrap;
}
.step-lbl.active {
    color: #e3c76b;
}
.step-lbl.done-lbl {
    color: rgba(208, 197, 181, 0.6);
}
.pickup-pill {
    display: flex;
    align-items: center;
    gap: 0.4rem;
    font-family: 'Work Sans', sans-serif;
    font-size: 0.7rem;
    letter-spacing: 0.1em;
    color: rgba(208, 197, 181, 0.6);
}
.pickup-pill strong {
    color: #e3c76b;
}

/* ════ Step 1 三欄佈局 ════ */
.out-layout {
    display: grid;
    grid-template-columns: 200px 1fr 360px;
    min-height: calc(100vh - var(--top-fixed, 153px));
    align-items: start;
}

/* ── Left Sidebar ── */
.out-sidebar {
    background: #180b06;
    border-right: 1px solid rgba(77, 70, 58, 0.2);
    position: sticky;
    top: var(--top-fixed, 153px);
    height: calc(100vh - var(--top-fixed, 153px));
    overflow-y: auto;
    display: flex;
    flex-direction: column;
}
.sidebar-search-wrap {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.85rem 1rem;
    border-bottom: 1px solid rgba(77, 70, 58, 0.2);
}
.sidebar-nav {
    flex: 1;
    padding: 0.5rem 0;
}
.cat-link {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 0.75rem;
    padding: 0.6rem 1.25rem;
    font-family: 'Work Sans', sans-serif;
    font-size: 1.2rem;
    letter-spacing: 0.14em;
    text-transform: uppercase;
    color: rgba(208, 197, 181, 0.6);
    border-left: 2px solid transparent;
    border-right: none;
    border-top: none;
    border-bottom: none;
    background: transparent;
    cursor: pointer;
    transition: all 0.3s;
    width: 100%;
}
.cat-link:hover {
    color: #e3c76b;
    background: rgba(227, 199, 107, 0.04);
}
.cat-link.active {
    color: #e3c76b;
    border-left-color: #e3c76b;
    background: rgba(227, 199, 107, 0.06);
}
.cat-link.cat-special {
    color: rgba(227, 199, 107, 0.75);
}
.cat-link.cat-special.active {
    color: #e3c76b;
}
.cat-count {
    margin-left: auto;
    font-size: 0.8rem;
    color: rgba(208, 197, 181, 0.35);
}
.cat-divider {
    height: 1px;
    background: linear-gradient(90deg, rgba(77, 70, 58, 0.5), transparent);
    margin: 0.3rem 1.25rem;
}

/* ── Centre: Menu ── */
.out-menu {
    background: #1e100b;
    min-height: calc(100vh - var(--top-fixed, 153px));
    overflow-x: hidden;
}
.menu-sections {
    padding: 0 1.5rem 4rem;
}

/* Toolbar */
.toolbar {
    position: sticky;
    top: 0; /* sticky 在 .out-menu 捲動容器內 */
    z-index: 20;
    background: #1e100b;
    border-bottom: 1px solid rgba(77, 70, 58, 0.25);
    padding: 0.85rem 1.25rem;
}
.mobile-search-wrap {
    display: none;
    align-items: center;
    gap: 0.5rem;
    margin-bottom: 0.5rem;
}
.toolbar-row {
    display: flex;
    align-items: center;
    gap: 0.75rem;
    flex-wrap: nowrap;
}
.chips-wrap {
    display: flex;
    gap: 0.4rem;
    overflow-x: auto;
    flex: 1 1 0;
    min-width: 0;
    scrollbar-width: none;
}
.chips-wrap::-webkit-scrollbar {
    display: none;
}
.view-toggle {
    display: flex;
    flex-shrink: 0;
}

/* ── Right: Cart ── */
.out-cart {
    background: #180b06;
    border-left: 1px solid rgba(77, 70, 58, 0.2);
    position: sticky;
    top: var(--top-fixed, 153px);
    height: calc(100vh - var(--top-fixed, 153px));
    display: flex;
    flex-direction: column;
    overflow: hidden;
}
.cart-header {
    flex-shrink: 0;
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 0.75rem 1rem;
    border-bottom: 1px solid rgba(77, 70, 58, 0.2);
    background: radial-gradient(
        ellipse 80% 60% at 50% 0%,
        rgba(227, 199, 107, 0.1) 0%,
        transparent 70%
    );
}
.cart-close-btn {
    display: none;
    background: none;
    border: none;
    cursor: pointer;
    color: rgba(208, 197, 181, 0.7);
    font-size: 1.4rem;
    line-height: 1;
    padding: 0;
}
.cart-pickup {
    flex-shrink: 0;
    padding: 0.75rem 1rem;
    border-bottom: 1px solid rgba(77, 70, 58, 0.15);
    display: flex;
    flex-direction: column;
    gap: 0.35rem;
}
.pickup-label {
    display: flex;
    align-items: center;
    gap: 0.35rem;
    font-size: 0.72rem;
    letter-spacing: 0.16em;
    text-transform: uppercase;
    color: rgba(208, 197, 181, 0.55);
}
.pickup-label.pickup-error {
    color: #ffb4ab;
}
.pickup-select {
    width: 100%;
    background: #2b1c16;
    border: 1px solid rgba(77, 70, 58, 0.5);
    border-radius: 0.25rem;
    color: #f9ddd3;
    font-size: 0.85rem;
    padding: 0.4rem 0.6rem;
    cursor: pointer;
    outline: none;
    transition: border-color 0.25s;
}
.pickup-select:focus {
    border-color: #e3c76b;
}
.pickup-select-error {
    border-color: #ffb4ab !important;
}
.pickup-select option {
    background: #2b1c16;
}
.pickup-error-msg {
    font-size: 0.7rem;
    color: #ffb4ab;
    margin: 0;
    letter-spacing: 0.06em;
}
.cart-items {
    flex: 1;
    min-height: 0;
    overflow-y: auto;
    padding: 0.25rem 1rem;
}
.cart-footer {
    flex-shrink: 0;
    border-top: 1px solid rgba(77, 70, 58, 0.2);
}

/* ════ Steps 2 & 3 ════ */
.step-page {
    min-height: calc(100vh - 3.5rem);
    display: flex;
    justify-content: center;
    align-items: flex-start;
    padding: 2.5rem 1rem 4rem;
}
.step-card {
    width: 100%;
    max-width: 560px;
    background: #271813;
    border: 1px solid rgba(77, 70, 58, 0.4);
    border-radius: 0.75rem;
    padding: 2rem;
}
.step-card-title {
    font-size: 1.7rem;
    font-style: italic;
    color: #e3c76b;
    margin: 0 0 1.5rem;
}

/* Form */
.form-group {
    display: flex;
    flex-direction: column;
    gap: 0.35rem;
    margin-bottom: 1.25rem;
}
.form-label {
    font-size: 0.72rem;
    letter-spacing: 0.18em;
    text-transform: uppercase;
    color: rgba(208, 197, 181, 0.55);
    display: flex;
    align-items: center;
    gap: 0.25rem;
}
.form-label.form-label-error {
    color: #ffb4ab;
}
.required-mark {
    color: #ffb4ab;
}
.autofill-hint {
    margin-left: 0.5rem;
    font-size: 0.68rem;
    letter-spacing: 0.08em;
    color: rgba(227, 199, 107, 0.55);
}
.form-input {
    width: 100%;
    background: #1e100b;
    border: 1px solid rgba(77, 70, 58, 0.5);
    border-radius: 0.25rem;
    color: #f9ddd3;
    font-size: 0.95rem;
    padding: 0.6rem 0.85rem;
    outline: none;
    transition: border-color 0.25s;
    font-style: italic;
}
.form-input:focus {
    border-color: #e3c76b;
}
.form-input.input-error {
    border-color: #ffb4ab;
}
.form-input::placeholder {
    color: rgba(208, 197, 181, 0.3);
}
.form-error-msg {
    font-size: 0.7rem;
    color: #ffb4ab;
    margin: 0;
}
.form-textarea {
    width: 100%;
    resize: vertical;
    background: #1e100b;
    border: 1px solid rgba(77, 70, 58, 0.5);
    border-radius: 0.25rem;
    color: #f9ddd3;
    font-size: 0.92rem;
    padding: 0.6rem 0.85rem;
    outline: none;
    transition: border-color 0.25s;
    font-style: italic;
    font-family: 'Newsreader', serif;
}
.form-textarea:focus {
    border-color: rgba(227, 199, 107, 0.5);
}
.form-textarea::placeholder {
    color: rgba(208, 197, 181, 0.3);
}

/* ── 餐具勾選 ── */
.utensils-group {
    margin-top: 0.25rem;
}
.utensils-label {
    display: flex;
    align-items: center;
    gap: 0.6rem;
    cursor: pointer;
    user-select: none;
    color: rgba(208, 197, 181, 0.85);
    font-size: 0.85rem;
    letter-spacing: 0.1em;
}
.utensils-checkbox {
    appearance: none;
    -webkit-appearance: none;
    width: 18px;
    height: 18px;
    border: 1px solid rgba(77, 70, 58, 0.7);
    border-radius: 3px;
    background: rgba(24, 11, 6, 0.5);
    cursor: pointer;
    flex-shrink: 0;
    position: relative;
    transition:
        border-color 0.2s,
        background 0.2s;
}
.utensils-checkbox:checked {
    background: #e3c76b;
    border-color: #e3c76b;
}
.utensils-checkbox:checked::after {
    content: '';
    position: absolute;
    left: 4px;
    top: 1px;
    width: 6px;
    height: 10px;
    border: 2px solid #3b2f00;
    border-top: none;
    border-left: none;
    transform: rotate(45deg);
}

.pickup-display {
    display: flex;
    align-items: center;
    gap: 0.4rem;
    font-size: 0.9rem;
    color: #e3c76b;
    letter-spacing: 0.06em;
    padding: 0.5rem 0.85rem;
    background: rgba(227, 199, 107, 0.06);
    border-radius: 0.25rem;
    border: 1px solid rgba(227, 199, 107, 0.2);
}
.step2-summary {
    margin-top: 0.5rem;
    background: rgba(24, 11, 6, 0.6);
    border: 1px solid rgba(77, 70, 58, 0.35);
    border-radius: 0.4rem;
    padding: 0.85rem 1rem;
    display: flex;
    flex-direction: column;
    gap: 0.4rem;
}
.step2-total-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    border-top: 1px solid rgba(77, 70, 58, 0.3);
    padding-top: 0.5rem;
    margin-top: 0.2rem;
}

/* Step Nav */
.step-nav {
    display: flex;
    gap: 0.75rem;
    margin-top: 1.5rem;
}
.step-back-btn {
    flex: 0 0 auto;
    padding: 0.75rem 1.25rem;
    background: transparent;
    border: 1px solid rgba(77, 70, 58, 0.6);
    color: rgba(208, 197, 181, 0.6);
    border-radius: 0.25rem;
    font-size: 0.8rem;
    letter-spacing: 0.12em;
    cursor: pointer;
    transition: all 0.25s;
}
.step-back-btn:hover {
    border-color: rgba(208, 197, 181, 0.5);
    color: rgba(208, 197, 181, 0.9);
}
.step-next-btn {
    flex: 1;
    font-size: 0.9rem !important;
    padding: 0.75rem 0 !important;
}

/* Confirm items */
.confirm-info {
    background: rgba(24, 11, 6, 0.5);
    border: 1px solid rgba(77, 70, 58, 0.35);
    border-radius: 0.4rem;
    padding: 0.85rem 1rem;
    display: flex;
    flex-direction: column;
    gap: 0.4rem;
}
.confirm-info-row {
    display: flex;
    gap: 1rem;
    align-items: baseline;
}
.confirm-info-label {
    font-size: 0.72rem;
    letter-spacing: 0.14em;
    text-transform: uppercase;
    color: rgba(208, 197, 181, 0.45);
    min-width: 4.5rem;
    flex-shrink: 0;
}
.confirm-info-val {
    font-size: 0.95rem;
    color: #f9ddd3;
}
.confirm-items {
    display: flex;
    flex-direction: column;
    gap: 0;
}
.confirm-item {
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: 0.75rem;
    padding: 0.6rem 0;
    border-bottom: 1px solid rgba(77, 70, 58, 0.2);
}
.confirm-item:last-child {
    border-bottom: none;
}
.confirm-item-left {
    flex: 1;
    min-width: 0;
}
.confirm-item-name {
    font-size: 0.95rem;
    color: #f9ddd3;
}
.confirm-item-right {
    display: flex;
    flex-direction: column;
    align-items: flex-end;
    gap: 0.15rem;
    flex-shrink: 0;
}
.confirm-totals {
    display: flex;
    flex-direction: column;
    gap: 0.45rem;
}
.confirm-total-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    font-size: 0.9rem;
}
/* 三欄列：標籤 | 名稱（彈性） | 金額 */
.confirm-total-row-3 {
    display: flex;
    align-items: center;
    gap: 0.45rem;
    font-size: 0.9rem;
}
.confirm-total-row-3 .confirm-dim {
    flex-shrink: 0;
    min-width: 3rem;
}
.confirm-mid {
    flex: 1;
    color: #f9ddd3;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}
.confirm-green {
    flex-shrink: 0;
    color: #7ec87e;
}
.confirm-dim {
    color: rgba(208, 197, 181, 0.6);
}
.confirm-grand {
    border-top: 1px solid rgba(77, 70, 58, 0.3);
    padding-top: 0.6rem;
    margin-top: 0.2rem;
}

/* ════ 訂單成功頁（Step 4）════ */

/* 整頁容器 */
.sp-wrap {
    min-height: 80vh;
    padding: 2rem 2.5rem 4rem;
    max-width: 1000px;
    margin: 0 auto;
    display: flex;
    flex-direction: column;
    gap: 1.5rem;
}

/* ① 頂部橫排：✓ | 標題 | 訂單編號 */
.sp-header-row {
    display: flex;
    align-items: center;
    gap: 1.25rem;
}
.sp-check-ring {
    flex-shrink: 0;
    width: 64px;
    height: 64px;
    border-radius: 50%;
    background: linear-gradient(135deg, #e3c76b, #c6ab53);
    display: flex;
    align-items: center;
    justify-content: center;
    color: #3b2f00;
    box-shadow: 0 8px 32px rgba(227, 199, 107, 0.35);
    animation: sp-pop 0.4s cubic-bezier(0.34, 1.56, 0.64, 1) both;
}
@keyframes sp-pop {
    from {
        transform: scale(0);
        opacity: 0;
    }
    to {
        transform: scale(1);
        opacity: 1;
    }
}
.sp-main-title {
    flex: 1;
    font-size: 2.2rem;
    font-style: italic;
    color: #e3c76b;
    margin: 0;
    white-space: nowrap;
}
.sp-order-num-block {
    display: flex;
    flex-direction: column;
    align-items: flex-end;
    gap: 0.15rem;
}
.sp-order-num-label {
    font-size: 0.9rem;
    letter-spacing: 0.18em;
    color: rgba(208, 197, 181, 0.45);
}
.sp-order-num-val {
    font-family: 'Work Sans', sans-serif;
    font-size: 1.5rem;
    letter-spacing: 0.15em;
    color: #f9ddd3;
    font-weight: 600;
}

/* ② 動態進度條（全寬） */
.sp-progress-bar {
    display: flex;
    align-items: center;
    width: 100%;
}
.sp-prog-step {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 0.4rem;
    flex: 0 0 auto;
    min-width: 100px;
}
.sp-prog-dot {
    width: 44px;
    height: 44px;
    border-radius: 50%;
    border: 2px solid rgba(77, 70, 58, 0.45);
    background: #271813;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 1rem;
    color: rgba(208, 197, 181, 0.4);
    transition:
        background 0.4s,
        border-color 0.4s,
        color 0.4s;
}
.sp-prog-step.sp-prog-done .sp-prog-dot {
    background: linear-gradient(135deg, #e3c76b, #c6ab53);
    border-color: #e3c76b;
    color: #3b2f00;
}
.sp-prog-step.sp-prog-active .sp-prog-dot {
    border-color: #e3c76b;
    background: rgba(227, 199, 107, 0.12);
    color: #e3c76b;
    animation: sp-pulse 1.4s ease-in-out infinite;
}
@keyframes sp-pulse {
    0%,
    100% {
        box-shadow: 0 0 0 0 rgba(227, 199, 107, 0.4);
    }
    50% {
        box-shadow: 0 0 0 10px rgba(227, 199, 107, 0);
    }
}
.sp-prog-lbl {
    font-size: 0.82rem;
    letter-spacing: 0.06em;
    color: rgba(208, 197, 181, 0.45);
    white-space: nowrap;
}
.sp-prog-step.sp-prog-done .sp-prog-lbl,
.sp-prog-step.sp-prog-active .sp-prog-lbl {
    color: #e3c76b;
}
.sp-prog-line {
    flex: 1;
    height: 2px;
    background: rgba(77, 70, 58, 0.45);
    margin-bottom: 1.5rem;
    transition: background 0.5s;
}
.sp-prog-line.sp-prog-line-lit {
    background: linear-gradient(90deg, #e3c76b, #c6ab53);
}

/* ③ 雙欄主體 */
.sp-body {
    display: grid;
    grid-template-columns: 1fr 0.72fr;
    gap: 1.25rem;
    align-items: stretch; /* 兩欄等高 */
}
.sp-col-left {
    display: flex;
    flex-direction: column;
    min-height: 0; /* 讓 flex 子元素可以收縮 */
}
.sp-col-right {
    display: flex;
    flex-direction: column;
    gap: 1rem;
}

/* Section 卡片 */
.sp-section {
    background: #271813;
    border: 1px solid rgba(77, 70, 58, 0.45);
    border-radius: 0.6rem;
    padding: 1.4rem 1.4rem 1.1rem;
}

/* 左欄的 section 撐滿整欄高度，items 區域可捲動 */
.sp-col-left .sp-section {
    flex: 1;
    display: flex;
    flex-direction: column;
    overflow: hidden;
    min-height: 0;
}
.sp-col-left .sp-items {
    flex: 1;
    overflow-y: auto;
    min-height: 0;
    /* 細緻捲軸 */
    scrollbar-width: thin;
    scrollbar-color: rgba(180, 120, 30, 0.35) transparent;
}
.sp-col-left .sp-items::-webkit-scrollbar {
    width: 4px;
}
.sp-col-left .sp-items::-webkit-scrollbar-track {
    background: transparent;
}
.sp-col-left .sp-items::-webkit-scrollbar-thumb {
    background: rgba(180, 120, 30, 0.35);
    border-radius: 2px;
}
.sp-section-title {
    font-size: 0.75rem;
    letter-spacing: 0.22em;
    text-transform: uppercase;
    color: rgba(208, 197, 181, 0.45);
    margin: 0 0 1rem;
}

/* 品項 */
.sp-items {
    display: flex;
    flex-direction: column;
}
.sp-item {
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: 1rem;
    padding: 0.65rem 0;
    border-bottom: 1px solid rgba(77, 70, 58, 0.2);
}
.sp-item:last-child {
    border-bottom: none;
}
.sp-item-left {
    flex: 1;
}
.sp-item-right {
    display: flex;
    flex-direction: column;
    align-items: flex-end;
    gap: 0.2rem;
    flex-shrink: 0;
}
.sp-item-name {
    font-size: 1rem;
    color: #f9ddd3;
    display: block;
}
.sp-item-note {
    font-size: 0.82rem;
    color: rgba(208, 197, 181, 0.42);
    margin: 0.15rem 0 0;
    display: block;
}
.sp-item-qty {
    font-size: 0.85rem;
    color: rgba(208, 197, 181, 0.5);
}
.sp-item-price {
    font-size: 0.95rem;
    color: #d5b478;
}

/* 金額 / 付款 / 取餐方式 */
.sp-meta-rows {
    display: flex;
    flex-direction: column;
    gap: 0.55rem;
}
.sp-meta-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    font-size: 0.95rem;
}
/* 三欄列：標籤 | 名稱（彈性） | 金額 */
.sp-meta-row-3 {
    display: flex;
    align-items: center;
    font-size: 0.95rem;
    gap: 0.5rem;
}
.sp-meta-row-3 .sp-meta-label {
    flex-shrink: 0;
    min-width: 3.2rem;
}
.sp-meta-val-mid {
    flex: 1;
    color: #f9ddd3;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}
.sp-meta-discount {
    flex-shrink: 0;
    color: #7ec87e;
}
/* 贈品活動來源標註 */
.sp-gift-event-note {
    font-size: 0.78rem;
    color: rgba(126, 200, 126, 0.75);
    padding: 0.25rem 0.5rem;
    letter-spacing: 0.03em;
}
.sp-meta-label {
    color: rgba(208, 197, 181, 0.5);
}
.sp-meta-val {
    color: #f9ddd3;
}
.sp-meta-gold {
    color: #e3c76b;
    font-size: 1.1rem;
}

/* 預計取餐時間 Banner */
.sp-pickup-banner {
    background: rgba(24, 11, 6, 0.55);
    border: 1px solid rgba(227, 199, 107, 0.25);
    border-radius: 0.5rem;
    padding: 1rem 1.25rem;
    display: flex;
    flex-direction: column;
    gap: 0;
}
/* 上排：標籤左、時間右 */
.sp-pickup-top {
    display: flex;
    align-items: center;
    justify-content: space-between;
}
/* 下排：付款方式 / 取餐方式 */
.sp-pickup-meta {
    border-top: 1px solid rgba(227, 199, 107, 0.12);
    margin-top: 0.7rem;
    padding-top: 0.6rem;
    display: flex;
    flex-direction: column;
    gap: 0.25rem;
}
.sp-pickup-label {
    font-size: 0.82rem;
    letter-spacing: 0.12em;
    color: rgba(208, 197, 181, 0.5);
}
.sp-pickup-time {
    font-size: 1.5rem;
    font-style: italic;
    color: #e3c76b;
}

/* 店家資訊 */
.sp-store-info {
    display: flex;
    flex-direction: column;
    gap: 0.4rem;
}
.sp-store-addr {
    font-size: 1rem;
    color: #d0c5b5;
    margin: 0;
}
.sp-store-tel {
    font-size: 1rem;
    color: #e3c76b;
    text-decoration: none;
    display: inline-flex;
    align-items: center;
    gap: 0.3rem;
}
.sp-store-tel:hover {
    text-decoration: underline;
}

/* 防呆提醒 */
.sp-reminders {
    background: rgba(24, 11, 6, 0.35);
    border: 1px solid rgba(77, 70, 58, 0.3);
    border-radius: 0.5rem;
    padding: 1rem 1.1rem;
    display: flex;
    flex-direction: column;
    gap: 0.65rem;
}
.sp-reminder-item {
    display: flex;
    align-items: flex-start;
    gap: 0.6rem;
    font-size: 0.88rem;
    color: rgba(208, 197, 181, 0.65);
    line-height: 1.5;
}
.sp-reminder-icon {
    flex-shrink: 0;
    font-size: 1rem;
}

/* 返回菜單按鈕 */
.sp-back-btn {
    width: 100%;
    padding: 1rem;
    background: linear-gradient(135deg, #e3c76b, #c6ab53);
    color: #3b2f00;
    border: none;
    border-radius: 0.3rem;
    font-size: 0.95rem;
    letter-spacing: 0.22em;
    text-transform: uppercase;
    cursor: pointer;
    transition: filter 0.2s;
}
.sp-back-btn:hover {
    filter: brightness(1.1);
}

/* 手機版：改回單欄 */
@media (max-width: 680px) {
    .sp-wrap {
        padding: 1.5rem 1rem 3rem;
    }
    .sp-header-row {
        flex-wrap: wrap;
        gap: 0.75rem;
    }
    .sp-main-title {
        font-size: 1.6rem;
    }
    .sp-order-num-block {
        align-items: flex-start;
    }
    .sp-body {
        grid-template-columns: 1fr;
        align-items: start; /* 手機單欄時還原自然高度 */
    }
    /* 手機版：左欄 section 改回自動高度，items 不捲動 */
    .sp-col-left .sp-section {
        flex: unset;
        overflow: visible;
    }
    .sp-col-left .sp-items {
        overflow-y: visible;
    }
    .sp-prog-step {
        min-width: 72px;
    }
    .sp-prog-dot {
        width: 36px;
        height: 36px;
    }
    .sp-prog-lbl {
        font-size: 0.72rem;
    }
}

/* ════ 手機底部列 ════ */
.mobile-bottom-bar {
    display: none;
}
.mobile-overlay {
    display: none;
}
.mobile-cat-bar {
    display: none;
}

/* ════ 通用元件 ════ */
.feather-divider {
    height: 1px;
    background: linear-gradient(90deg, transparent, #e4c285 50%, transparent);
    position: relative;
}
.feather-divider::after {
    content: '◈';
    position: absolute;
    left: 50%;
    top: 50%;
    transform: translate(-50%, -50%);
    color: #e4c285;
    font-size: 0.7rem;
    background: #1e100b;
    padding: 0 0.65rem;
}
.section-title {
    font-family: 'Noto Serif TC', serif;
    font-style: italic;
    font-size: 2rem;
    color: #e3c76b;
    padding-top: 1.5rem;
    margin-bottom: 0.85rem;
}
.status-msg {
    text-align: center;
    padding: 5rem 1rem;
    font-family: 'Newsreader', serif;
    font-style: italic;
    color: rgba(249, 221, 211, 0.4);
}

/* ═══ 歷史訂單 ═══ */
.history-card {
    background: rgba(43, 28, 22, 0.6);
    border: 1px solid rgba(77, 70, 58, 0.4);
    border-radius: 0.5rem;
    padding: 1rem 1.25rem;
    margin-bottom: 1rem;
    display: flex;
    flex-direction: column;
    gap: 0.6rem;
}
.history-meta {
    display: flex;
    flex-direction: column;
    gap: 0.15rem;
}
.history-items {
    display: flex;
    flex-direction: column;
    gap: 0.2rem;
}
.history-reorder-btn {
    align-self: flex-end;
    padding: 0.5rem 1.25rem;
    background: linear-gradient(to right, #e3c76b, #c6ab53);
    color: #3b2f00;
    border: none;
    border-radius: 0.375rem;
    font-size: 0.8rem;
    letter-spacing: 0.15em;
    cursor: pointer;
    transition: filter 0.2s;
}
.history-reorder-btn:hover {
    filter: brightness(1.08);
}
.input-line {
    background: transparent;
    border: none;
    border-bottom: 1px solid rgba(77, 70, 58, 0.5);
    color: #f9ddd3;
    outline: none;
    padding: 0.25rem 0.5rem;
    width: 100%;
    font-family: 'Newsreader', serif;
    font-style: italic;
    transition: border-color 0.25s;
}
.input-line:focus {
    border-bottom-color: rgba(227, 199, 107, 0.7);
}
.input-line::placeholder {
    color: rgba(208, 197, 181, 0.3);
}

/* 優惠券下拉選單 */
.coupon-select {
    width: 100%;
    background: rgba(24, 11, 6, 0.6);
    border: 1px solid rgba(77, 70, 58, 0.6);
    border-radius: 0.25rem;
    color: #f9ddd3;
    font-family: 'Newsreader', serif;
    font-size: 0.85rem;
    padding: 0.45rem 2rem 0.45rem 0.65rem;
    outline: none;
    cursor: pointer;
    transition: border-color 0.25s;
    appearance: none;
    background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='10' height='6' fill='none'%3E%3Cpath d='M1 1l4 4 4-4' stroke='%23e3c76b' stroke-width='1.5' stroke-linecap='round' stroke-linejoin='round'/%3E%3C/svg%3E");
    background-repeat: no-repeat;
    background-position: right 0.65rem center;
}
.coupon-select:focus {
    border-color: rgba(227, 199, 107, 0.6);
}
.coupon-select option {
    background: #2b1c16;
    color: #f9ddd3;
}
/* 訪客優惠券區：登入會員按鈕 */
.coupon-login-btn {
    width: 100%;
    padding: 0.5rem 0.75rem;
    background: transparent;
    border: 1px solid rgba(227, 199, 107, 0.45);
    border-radius: 0.25rem;
    color: #e3c76b;
    font-size: 0.75rem;
    letter-spacing: 0.14em;
    text-transform: uppercase;
    cursor: pointer;
    transition:
        background 0.2s,
        border-color 0.2s;
    text-align: center;
}
.coupon-login-btn:hover {
    background: rgba(227, 199, 107, 0.08);
    border-color: rgba(227, 199, 107, 0.7);
}

/* 下拉選單最底部「查看更多優惠券」特殊項目 */
.coupon-goto-option {
    color: #e3c76b;
    border-top: 1px solid rgba(77, 70, 58, 0.4);
}

.note-textarea {
    width: 100%;
    background: transparent;
    border: 1px solid rgba(77, 70, 58, 0.35);
    border-radius: 0.2rem;
    color: #f9ddd3;
    font-size: 0.88rem;
    padding: 0.4rem 0.6rem;
    font-family: 'Newsreader', serif;
    font-style: italic;
    outline: none;
    transition: border-color 0.25s;
}
.note-textarea:focus {
    border-color: rgba(227, 199, 107, 0.4);
}
.note-textarea::placeholder {
    color: rgba(208, 197, 181, 0.3);
}

/* Submit / Clear buttons */
.submit-btn {
    background: linear-gradient(135deg, #e3c76b 0%, #c6ab53 100%);
    color: #3b2f00;
    border: none;
    border-radius: 0.25rem;
    cursor: pointer;
    transition: filter 0.2s;
    letter-spacing: 0.18em;
}
.submit-btn:hover:not(:disabled) {
    filter: brightness(1.08);
}
.submit-btn:disabled {
    opacity: 0.45;
    cursor: not-allowed;
}
.clear-btn {
    background: transparent;
    border: 1px solid rgba(77, 70, 58, 0.4);
    color: rgba(208, 197, 181, 0.4);
    border-radius: 0.25rem;
    cursor: pointer;
    transition: all 0.25s;
}
.clear-btn:hover {
    border-color: rgba(255, 100, 100, 0.4);
    color: rgba(255, 160, 160, 0.6);
}

/* Dish rows */
.dish-row {
    display: grid;
    grid-template-columns: 160px 1fr;
    align-items: stretch;
    background: #362620;
    border-radius: 0.75rem;
    cursor: pointer;
    transition:
        transform 0.45s cubic-bezier(0.4, 0, 0.2, 1),
        box-shadow 0.45s ease;
}
.dish-row:hover {
    transform: translateY(-2px);
    box-shadow: 0 6px 20px rgba(0, 0, 0, 0.35);
}
.dish-row.dish-row-grid {
    grid-template-columns: 1fr;
    display: flex;
    flex-direction: column;
    overflow: hidden;
}
.dish-img {
    width: 160px;
    height: 120px;
    object-fit: cover;
    align-self: center;
    display: block;
    flex-shrink: 0;
    transition: transform 0.7s cubic-bezier(0.4, 0, 0.2, 1);
}
.dish-row:hover .dish-img {
    transform: scale(1.05);
}
.dish-row-grid .dish-img {
    width: 100%;
    height: 140px;
}
.dish-img-placeholder {
    width: 160px;
    height: 120px;
    align-self: center;
    background: #2b1c16;
    display: flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
    color: rgba(208, 197, 181, 0.15);
    font-size: 1.8rem;
}
.dish-row-grid .dish-img-placeholder {
    width: 100%;
    height: 140px;
}
.dish-content {
    display: flex;
    flex-direction: column;
    justify-content: center;
    padding: 0.75rem;
}
.dish-content-grid {
    flex: 1;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: flex-start;
    text-align: center;
}
.dish-name {
    font-size: 1.4rem;
}
.dish-row-grid .dish-name {
    font-size: 1.285rem;
    text-align: center;
    width: 100%;
    display: -webkit-box;
    -webkit-box-orient: vertical;
    overflow: hidden;
}
.dish-badges {
    display: flex;
    flex-wrap: wrap;
    gap: 0.3rem;
    margin: 0.4rem 0;
}
.dish-content-grid .dish-badges {
    justify-content: center;
}
.grid-footer {
    display: flex;
    flex-direction: column;
    align-items: center;
    padding: 0 0.75rem;
    border-top: 1px solid rgba(77, 70, 58, 0.25);
}
.grid-price {
    color: #d5b478;
    font-size: 1rem;
    letter-spacing: 0.08em;
    text-align: center;
}
.list-footer {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-top: auto;
    padding-top: 0.4rem;
}
.qty-col {
    display: flex;
    flex-direction: row;
    align-items: center;
    gap: 0.4rem;
}
.qty-row {
    display: flex;
    flex-direction: row;
    align-items: center;
    gap: 0.4rem;
    margin-bottom: 0.8rem;
    margin-top: auto;
}
.qty-num {
    color: #f9ddd3;
    width: 40px;
    text-align: center;
}
.qty-num.active {
    color: #e3c76b;
}
.dishes-wrap.grid-view {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
    gap: 0.75rem;
}
.qty-btn {
    width: 30px;
    height: 30px;
    border: 1px solid rgba(77, 70, 58, 0.7);
    background: #2b1c16;
    color: #f9ddd3;
    border-radius: 0.125rem;
    cursor: pointer;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    font-size: 1.5rem;
    line-height: 1;
    transition:
        border-color 0.3s,
        color 0.3s;
}
.qty-btn:hover {
    border-color: #e3c76b;
    color: #e3c76b;
}
.qty-btn-order {
    width: 24px;
    height: 24px;
    border: 1px solid rgba(77, 70, 58, 0.6);
    background: #2b1c16;
    color: #f9ddd3;
    border-radius: 0.125rem;
    cursor: pointer;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    font-size: 1.1rem;
    line-height: 1;
    transition:
        border-color 0.3s,
        color 0.3s;
    flex-shrink: 0;
}
.qty-btn-order:hover {
    border-color: #e3c76b;
    color: #e3c76b;
}
.chip-label {
    font-family: 'Work Sans', sans-serif;
    font-size: 0.7rem;
    letter-spacing: 0.15em;
    text-transform: uppercase;
    color: rgba(208, 197, 181, 0.45);
    white-space: nowrap;
    flex-shrink: 0;
    align-self: center;
}
.chip-btn {
    font-family: 'Work Sans', sans-serif;
    font-size: 1rem;
    letter-spacing: 0.15em;
    text-transform: uppercase;
    padding: 0.28rem 0.8rem;
    border-radius: 0.75rem;
    background: #5d4514;
    color: #e4c285;
    border: 1px solid transparent;
    cursor: pointer;
    transition: all 0.3s;
    user-select: none;
}
.chip-btn.active {
    background: rgba(227, 199, 107, 0.12);
    border-color: rgba(227, 199, 107, 0.45);
    color: #e3c76b;
}
.view-btn {
    width: 28px;
    height: 28px;
    background: transparent;
    border: 1px solid rgba(77, 70, 58, 0.5);
    color: rgba(208, 197, 181, 0.45);
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    transition: all 0.25s;
}
.view-btn.active {
    border-color: rgba(227, 199, 107, 0.5);
    color: #e3c76b;
    background: rgba(227, 199, 107, 0.06);
}

/* Order item 共用 */
.order-item {
    display: flex;
    flex-direction: column;
    gap: 0.15rem;
    padding: 0.5rem 0;
    border-bottom: 1px solid rgba(77, 70, 58, 0.25);
}
.order-item:last-child {
    border-bottom: none;
}
.setmeal-order-item {
    background: rgba(227, 199, 107, 0.03);
    border-left: 2px solid rgba(227, 199, 107, 0.25);
}
.setmeal-badge {
    font-size: 0.65rem;
    letter-spacing: 0.1em;
    color: rgba(227, 199, 107, 0.7);
    margin-bottom: 0.1rem;
}
.setmeal-subitems {
    display: flex;
    flex-wrap: wrap;
    gap: 0.25rem 0.5rem;
    margin-top: 0.15rem;
}
.setmeal-subitem {
    font-size: 0.68rem;
    color: rgba(208, 197, 181, 0.5);
    letter-spacing: 0.04em;
}
.gift-order-item {
    background: rgba(163, 217, 119, 0.04);
}
.gift-order-badge {
    font-size: 0.68rem;
    letter-spacing: 0.1em;
    color: #a3d977;
    background: rgba(163, 217, 119, 0.12);
    border: 1px solid rgba(163, 217, 119, 0.3);
    border-radius: 0.2rem;
    padding: 0.05rem 0.35rem;
    display: inline-block;
}
.order-item-top {
    display: flex;
    align-items: center;
    gap: 0.5rem;
}
.order-item-note {
    font-size: 1rem;
    color: rgba(208, 197, 181, 0.45);
    margin: 0;
    padding-left: 0.1rem;
    white-space: pre-wrap;
    word-break: break-all;
}
.order-item-name-wrap {
    flex: 1;
    min-width: 0;
    cursor: pointer;
    border-radius: 0.25rem;
    padding: 0.1rem 0.2rem;
    margin: -0.1rem -0.2rem;
    transition: background 0.15s;
}
.order-item-name-wrap:hover {
    background: rgba(227, 199, 107, 0.07);
}
.order-item-name {
    margin: 0.1rem 0;
    min-width: 0;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    font-size: 1rem;
}
.order-item-right {
    display: flex;
    align-items: center;
    gap: 0.4rem;
    flex-shrink: 0;
}
.order-item-price {
    padding-right: 0.6rem;
    font-size: 1rem;
    letter-spacing: 0.08em;
    color: #d5b478;
    white-space: nowrap;
}
.order-item-qty {
    font-size: 1rem;
    width: 1.5rem;
    text-align: center;
}
.candle-glow {
    background: radial-gradient(
        ellipse 80% 60% at 50% 0%,
        rgba(227, 199, 107, 0.1) 0%,
        transparent 70%
    );
}

/* 活動提示 */
.notify-events {
    margin-top: 0.5rem;
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
}
.notify-event-card {
    border-radius: 0.4rem;
    padding: 0.55rem 0.75rem;
    border: 1px solid rgba(77, 70, 58, 0.3);
    display: flex;
    flex-direction: column;
    gap: 0.25rem;
}
.notify-event-card.near-threshold {
    background: rgba(42, 30, 16, 0.8);
    border-color: rgba(255, 160, 80, 0.3);
}
.notify-event-card.eligible {
    background: rgba(30, 45, 26, 0.8);
    border-color: rgba(163, 217, 119, 0.35);
}
.notify-event-card.guest-login-hint {
    background: rgba(30, 25, 16, 0.8);
    border-color: rgba(227, 199, 107, 0.25);
}
.notify-event-top {
    display: flex;
    align-items: center;
    gap: 0.4rem;
}
.notify-event-icon {
    font-size: 0.9rem;
}
.notify-event-title {
    font-size: 0.72rem;
    letter-spacing: 0.1em;
    color: rgba(208, 197, 181, 0.8);
    flex: 1;
    min-width: 0;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}
.notify-event-badge {
    font-size: 0.62rem;
    letter-spacing: 0.08em;
    padding: 0.05rem 0.35rem;
    border-radius: 0.2rem;
}
.eligible-badge {
    background: rgba(163, 217, 119, 0.15);
    color: #a3d977;
    border: 1px solid rgba(163, 217, 119, 0.3);
}
.near-badge {
    background: rgba(255, 160, 80, 0.12);
    color: #ff9f4a;
    border: 1px solid rgba(255, 160, 80, 0.25);
}
.notify-event-desc {
    font-size: 0.75rem;
    color: rgba(208, 197, 181, 0.55);
    margin: 0;
    font-style: italic;
}
.applied-event-tag {
    display: flex;
    align-items: flex-start;
    gap: 0.4rem;
    background: rgba(227, 199, 107, 0.06);
    border: 1px solid rgba(227, 199, 107, 0.2);
    border-radius: 0.35rem;
    padding: 0.45rem 0.65rem;
    margin-bottom: 0.25rem;
}
.applied-event-icon {
    font-size: 0.85rem;
    flex-shrink: 0;
    margin-top: 0.05rem;
}
.applied-event-text {
    font-size: 0.72rem;
    color: rgba(227, 199, 107, 0.8);
    letter-spacing: 0.05em;
    line-height: 1.5;
}

/* Badge */
.badge {
    font-size: 0.62rem;
    letter-spacing: 0.1em;
    padding: 0.05rem 0.4rem;
    border-radius: 0.2rem;
    font-family: 'Work Sans', sans-serif;
}
.badge-new {
    background: rgba(227, 199, 107, 0.15);
    color: #e3c76b;
    border: 1px solid rgba(227, 199, 107, 0.3);
}
.badge-chef {
    background: rgba(200, 150, 60, 0.15);
    color: #c8963c;
    border: 1px solid rgba(200, 150, 60, 0.3);
}
.badge-veg {
    background: rgba(100, 200, 100, 0.15);
    color: #7dc97d;
    border: 1px solid rgba(100, 200, 100, 0.3);
}
.badge-spicy {
    background: rgba(230, 80, 60, 0.15);
    color: #e85040;
    border: 1px solid rgba(230, 80, 60, 0.25);
}

/* Bottom badge */
.bottom-badge {
    min-width: 1.5rem;
    height: 1.5rem;
    border-radius: 99px;
    background: linear-gradient(to right, #e3c76b, #c6ab53);
    color: #3b2f00;
    font-family: 'Work Sans', sans-serif;
    font-size: 0.7rem;
    font-weight: 700;
    display: flex;
    align-items: center;
    justify-content: center;
}
.bottom-cta {
    background: linear-gradient(to right, #e3c76b, #c6ab53);
    color: #3b2f00;
    padding: 0.45rem 1rem;
    border-radius: 0.125rem;
}

/* ════════════════════════════════════════════════════
   桌機網頁版 (min-width: 1101px)
   ════════════════════════════════════════════════════ */
@media (min-width: 1101px) {
    /* toolbar 固定在 navbar + step-banner 正下方 */
    .toolbar {
        top: var(--top-fixed, 153px);
    }
    /* 移除 overflow-x: hidden，讓 toolbar position:sticky 可正常運作 */
    .out-menu {
        overflow-x: visible;
    }
}

/* ════════════════════════════════════════════════════
   手機版 (max-width: 1100px)
   ════════════════════════════════════════════════════ */
@media (max-width: 1100px) {
    /* 單欄佈局，隱藏桌機側邊欄 */
    .out-layout {
        grid-template-columns: 1fr;
    }
    .out-sidebar {
        display: none;
    }

    /* 手機版分類列：固定在 step-banner 正下方 */
    .mobile-cat-bar {
        display: block;
        position: sticky;
        top: var(--top-fixed, 130px); /* navbar + step-banner 高度（JS 量測後注入） */
        z-index: 50;
        background: #180b06;
        border-bottom: 1px solid rgba(77, 70, 58, 0.3);
    }
    .mobile-cat-tabs {
        display: flex;
        overflow-x: auto;
        padding: 0.4rem 0.75rem 0.55rem;
        scrollbar-width: none;
        gap: 0;
    }
    .mobile-cat-tabs::-webkit-scrollbar {
        display: none;
    }
    .mobile-cat-btn {
        white-space: nowrap;
        padding: 0.3rem 0.85rem;
        font-family: 'Work Sans', sans-serif;
        font-size: 0.68rem;
        letter-spacing: 0.12em;
        text-transform: uppercase;
        color: rgba(208, 197, 181, 0.5);
        border: none;
        border-bottom: 2px solid transparent;
        background: transparent;
        cursor: pointer;
        transition: all 0.25s;
        display: flex;
        align-items: center;
        gap: 0.3rem;
    }
    .mobile-cat-btn.active {
        color: #e3c76b;
        border-bottom-color: #e3c76b;
    }
    .cat-badge {
        font-size: 0.55rem;
        background: rgba(77, 70, 58, 0.5);
        color: rgba(208, 197, 181, 0.5);
        border-radius: 99px;
        padding: 0.05rem 0.35rem;
    }

    /* 手機版搜尋列（toolbar 內）顯示 */
    .mobile-search-wrap {
        display: flex;
    }
    /* toolbar 固定在分類列正下方：step-banner 底部 + 分類列高度 */
    .toolbar {
        top: calc(var(--top-fixed, 130px) + var(--cat-bar-h, 40px));
    }
    .out-menu {
        min-height: auto;
    }
    .out-cart {
        position: fixed;
        inset: 0;
        z-index: 200;
        transform: translateY(100%);
        transition: transform 0.35s cubic-bezier(0.4, 0, 0.2, 1);
        border-left: none;
        border-top: 1px solid rgba(77, 70, 58, 0.4);
        height: 100%;
        top: 0;
    }
    .out-cart.cart-open {
        transform: translateY(0);
    }
    .cart-close-btn {
        display: block;
    }
    .mobile-bottom-bar {
        display: flex;
        align-items: center;
        justify-content: space-between;
        position: fixed;
        bottom: 0;
        left: 0;
        right: 0;
        z-index: 80;
        background: linear-gradient(135deg, #2b1c16 0%, #1e100b 100%);
        border-top: 1px solid rgba(77, 70, 58, 0.4);
        padding: 0.85rem 1.25rem;
        cursor: pointer;
        box-shadow: 0 -4px 20px rgba(0, 0, 0, 0.4);
    }
    .mobile-overlay {
        display: block;
        position: fixed;
        inset: 0;
        z-index: 100;
        background: rgba(24, 11, 6, 0.78);
        pointer-events: auto;
    }
    .menu-sections {
        padding: 0 1rem 6rem;
    }
    .step-page {
        padding: 1.5rem 1rem 3rem;
    }
    .step-card {
        padding: 1.5rem;
    }
}

/* ════════════════════════════════════════════════════
   手機版小螢幕微調 (max-width: 560px)
   ════════════════════════════════════════════════════ */
@media (max-width: 560px) {
    /* step-banner 縮短左右 padding */
    .step-banner {
        padding: 0.5rem 1rem;
    }
    /* 進度圓點縮小 */
    .step-dot {
        width: 30px;
        height: 30px;
        font-size: 0.8rem;
    }
    /* 只顯示目前步驟的 label，其餘隱藏 */
    .step-lbl {
        display: none;
    }
    .step-lbl.active {
        display: inline;
        font-size: 0.72rem;
        letter-spacing: 0.08em;
    }
    .step-card-title {
        font-size: 1.4rem;
    }
}
</style>
