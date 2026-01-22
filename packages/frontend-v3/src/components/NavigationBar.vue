<template>
	<div class="navigation-bar">
		<div
			v-if="!isMobile"
			class="navigation-container"
		>
			<div class="menu-container">
				<div>
					<router-link
						to="/"
						class="logo-link"
					>
						<img
							class="logo"
							src="@/assets/alv.svg"
						/>
					</router-link>
				</div>
				<div>
					<router-link
						to="/"
					>
						Timeføring er lættis
					</router-link>
				</div>
				<div>
					<router-link
						to="/aktiviteter"
					>
						Aktiviteter
					</router-link>
				</div>
				<div>
					<router-link
						to="/timebank"
					>
						Timebank
					</router-link>
				</div>
			</div>
			<div class="user-container">
				<p>{{ user?.name }}</p>
				<LogOutButton />
			</div>
		</div>
		<div
			v-else
			class="navigation-container mobile"
		>
			<div>
				<router-link
					to="/"
					class="logo-link"
				>
					<img
						class="logo"
						src="@/assets/alv.svg"
					/>
				</router-link>
			</div>
			<div>
				<FeatherIcon
					name="menu"
					:size="32"
					@click="open = !open"
				/>
			</div>
		</div>
	</div>
	<MobileMenu
		v-if="isMobile"
		:open="open"
		@close="close"
	/>
</template>

<script setup lang="ts">
import { ref } from "vue";
import LogOutButton from "@/components/LogOutButton.vue";
import { useUserStore } from "@/stores/userStore";
import FeatherIcon from "./utils/FeatherIcon.vue";
import MobileMenu from "./utils/MobileMenu.vue";

const open = ref(false);
const { user } = useUserStore();
const isMobile = window.innerWidth <= 768;

const close = () => {
	open.value = false;
};
</script>

<style scoped lang="scss">
.navigation-bar {
	display: flex;
	justify-content: center;
	align-items: center;
	height: 80px;
	background-color: rgb(36, 63, 77);
	color: $background-color;

	.navigation-container {
		width: 1200px;
		padding: 0 20px;
		display: flex;
		justify-content: space-between;
		align-items: center;

		a {
			text-decoration: none;
			margin-right: 20px;

			&:hover {
				text-decoration: underline;
			}
		}

		p {
			text-align: center;
		}

		.menu-container {
			display: flex;
			align-items: center;
			gap: 20px;

			a {
				color: $background-color;
				font-size: 18px;
				cursor: pointer;
				&:hover {
					text-decoration: underline;
				}
			}

			.logo-link {
				margin-right: 3rem;
			}
		}

		.user-container {
			display: flex;
			justify-content: flex-end;
			align-items: center;
			gap: 24px;

			p {
				margin: 0;
			}
		}
	}

	.logo {
		height: 32px;
	}
}
</style>