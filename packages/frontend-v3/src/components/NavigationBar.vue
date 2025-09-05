<template>
	<div class="navigation-bar">
		<div class="navigation-container">
			<div class="menu-container">
				<router-link
					to="/"
					class="logo-link"
				>
					<img
						class="logo"
						src="@/assets/alv.svg"
					/>
				</router-link>
				<router-link
					to="/"
				>
					Timeføring
				</router-link>
				<router-link
					to="/aktiviteter"
				>
					Aktiviteter
				</router-link>
				<router-link
					to="/timebank"
				>
					Timebank
				</router-link>
			</div>
			<div class="user-container">
				<p>{{ user?.name }}</p>
				<LogOutButton />
			</div>
		</div>
	</div>
</template>

<script setup lang="ts">
import LogOutButton from "@/components/LogOutButton.vue";
import { useUserStore } from "@/stores/userStore";

const { user } = useUserStore();
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