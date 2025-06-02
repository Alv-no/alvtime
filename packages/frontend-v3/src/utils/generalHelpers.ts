// This function debounces a function call, ensuring that it is not called too frequently.
const debounce = (func: (...args: any[]) => void, delay: number) => {
	let timeoutId: ReturnType<typeof setTimeout>;
	return (...args: any[]) => {
		clearTimeout(timeoutId);
		timeoutId = setTimeout(() => func(...args), delay);
	};
};

export { debounce };