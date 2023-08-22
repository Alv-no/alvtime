export const getCustomers = async ({ token }: { token: string }) => {
	console.log('getting customers');

	const res = await fetch('http://localhost:8081/api/admin/Customes', {
		method: 'GET',
		headers: {
			Authorization: `Bearer ${token}`,
			'content-type': 'application/json'
		}
	});
	console.log(res.ok);

	return await res.json();
};
