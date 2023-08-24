export const createProject = async ({ body, token }) => {
	const res = await fetch('http://localhost:8081/api/admin/Projects', {
		method: 'POST',
		headers: {
			Authorization: `Bearer ${token}`,
			'content-type': 'application/json'
		},
		body: JSON.stringify(body)
	});

	return await res.json();
};

export const updateProject = async ({ body, token }) => {
	const res = await fetch('http://localhost:8081/api/admin/Projects', {
		method: 'PUT',
		headers: {
			Authorization: `Bearer ${token}`,
			'content-type': 'application/json'
		},
		body: JSON.stringify(body)
	});

	return await res.json();
};

export const getProjects = async ({ token }) => {
	const res = await fetch(`http://localhost:8081/api/admin/Projects`, {
		method: 'GET',
		headers: {
			Authorization: `Bearer ${token}`
		}
	});

	return await res.json();
};
