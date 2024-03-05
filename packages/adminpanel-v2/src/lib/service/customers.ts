import type { TCustomer } from '$lib/types';

export const createCustomer = async ({
	body,
	token
}: {
	body: Partial<TCustomer>;
	token: string;
}) => {
	const res = await fetch('http://localhost:8081/api/admin/Customers', {
		method: 'POST',
		headers: {
			Authorization: `Bearer ${token}`,
			'content-type': 'application/json'
		},
		body: JSON.stringify(body)
	});
	console.log(res.ok);

	return await res.json();
};

export const updateCustomer = async ({
	body,
	token
}: {
	body: Partial<TCustomer>;
	token: string;
}) => {
	const res = await fetch('http://localhost:8081/api/admin/Customers', {
		method: 'PUT',
		headers: {
			Authorization: `Bearer ${token}`,
			'content-type': 'application/json'
		},
		body: JSON.stringify(body)
	});

	return await res.json();
};

export const fetchCustomers = async ({ token }: { token: string }) => {
	const res = await fetch(`http://localhost:8081/api/admin/Customers`, {
		method: 'GET',
		headers: {
			Authorization: `Bearer ${token}`
		}
	});

	return (await res.json()) as TCustomer[];
};
