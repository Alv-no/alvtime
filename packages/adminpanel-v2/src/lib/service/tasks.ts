export const createTask = async ({ body, token }) => {
	const res = await fetch('http://localhost:8081/api/admin/Task', {
		method: 'POST',
		headers: {
			Authorization: `Bearer ${token}`,
			'content-type': 'application/json'
		},
		body: JSON.stringify(body)
	});

	return await res.json();
};

export const updateTask = async ({ body, token }) => {
	const res = await fetch('http://localhost:8081/api/admin/Task', {
		method: 'PUT',
		headers: {
			Authorization: `Bearer ${token}`,
			'content-type': 'application/json'
		},
		body: JSON.stringify(body)
	});

	return await res.json();
};

export const getTasks = async ({ token }) => {
	const res = await fetch(`http://localhost:8081/api/admin/Tasks`, {
		method: 'GET',
		headers: {
			Authorization: `Bearer ${token}`
		}
	});

	return await res.json();
};
