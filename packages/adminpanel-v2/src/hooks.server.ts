import { redirect, type Handle } from '@sveltejs/kit';
import { sequence } from '@sveltejs/kit/hooks';
import { SvelteKitAuth } from '@auth/sveltekit';
import AzureAD from '@auth/core/providers/azure-ad';
import CredentialsProvider from '@auth/core/providers/credentials';
import {
	AZURE_AD_CLIENT_ID,
	AZURE_AD_TENANT_ID,
	AUTH_SECRET,
	AZURE_AD_SCOPE,
	AZURE_AD_CLIENT_SECRET
} from '$env/static/private';

const handleProtectedRoutes = async ({ event, resolve }) => {
	const session = await event.locals.getSession();
	console.log('HANDLE PROTECTED', event.url.pathname);
	// Check protected routes
	if (event.url.pathname.startsWith('/customers') && !session) {
		throw redirect(303, '/login');
	}
	if (event.url.pathname.startsWith('/employees') && !session) {
		throw redirect(303, '/login');
	}
	if (event.url.pathname.startsWith('/login') && session) {
		throw redirect(303, '/');
	}
	return resolve(event);
};

const refreshAccessToken = async (accessToken: any) => {
	//if devmode return
	if (process.env.NODE_ENV === 'development') {
		return {
			...accessToken,
			accessToken: '5801gj90-jf39-5j30-fjk3-480fj39kl409',
			accessTokenExpires: Date.now() * 1000,
			refreshToken: 'prank'
		};
	}

	try {
		const url = `https://login.microsoftonline.com/${AZURE_AD_TENANT_ID}/oauth2/v2.0/token`;
		const req = await fetch(url, {
			method: 'POST',
			headers: {
				'Content-Type': 'application/x-www-form-urlencoded',
				Origin: 'http://localhost:5173'
			},
			body:
				`grant_type=refresh_token` +
				`&client_secret=${AZURE_AD_CLIENT_SECRET}` +
				`&refresh_token=${accessToken.refreshToken}` +
				`&client_id=${AZURE_AD_CLIENT_ID}`
		});

		const res = await req.json();
		return {
			...accessToken,
			accessToken: res.access_token,
			accessTokenExpires: Date.now() + res.expires_in * 1000,
			refreshToken: res.refresh_token ?? accessToken.refreshToken // Fall backto old refresh token
		};
	} catch (error) {
		console.error('Error refreshing access token', error);
		// The error property will be used client-side to handle the refresh token error
		return { ...accessToken, error: 'RefreshAccessTokenError' };
	}
};

const handleAuth = SvelteKitAuth({
	providers: [
		CredentialsProvider({
			// The name to display on the sign in form (e.g. "Sign in with...")
			name: 'Credentials',
			// `credentials` is used to generate a form on the sign in page.
			// You can specify which fields should be submitted, by adding keys to the `credentials` object.
			// e.g. domain, username, password, 2FA token, etc.
			// You can pass any HTML attribute to the <input> tag through the object.
			credentials: {
				username: { label: 'Username', type: 'text', placeholder: 'test' }
			},
			async authorize(credentials, req) {
				return {
					id: '1'
				};
			}
		}),
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		AzureAD({
			clientId: AZURE_AD_CLIENT_ID,
			tenantId: AZURE_AD_TENANT_ID,
			clientSecret: AZURE_AD_CLIENT_SECRET,
			authorization: {
				params: {
					scope: `openid profile ${AZURE_AD_SCOPE} email offline_access`
				}
			}
		})
	],
	secret: AUTH_SECRET,
	trustHost: true,
	callbacks: {
		async jwt({ token, profile, account }) {
			// console.log({ token, profile, account })

			if (account) {
				token.accessToken = account.access_token;
				token.refreshToken = account.refresh_token;
			}
			if (profile) {
				token.roles = profile.roles;
			}
			if (Date.now() < token.accessTokenExpires) {
				return token;
			}
			return refreshAccessToken(token);
		},
		async session({ session, user, token }) {
			session.accessToken = token.accessToken;
			session.user.roles = token.roles;
			return session;
		}
	}
}) satisfies Handle;

export const handle = sequence(handleAuth, handleProtectedRoutes);
