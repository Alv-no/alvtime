/* eslint-disable @typescript-eslint/no-explicit-any */
// This function debounces a function call, ensuring that it is not called too frequently.
// eslint-disable-next-line @typescript-eslint/no-unsafe-function-type
const debounce = (fn: Function, ms = 300) => {
	let timeoutId: ReturnType<typeof setTimeout>;
	return function (this: any, ...args: any[]) {
		clearTimeout(timeoutId);
		timeoutId = setTimeout(() => fn.apply(this, args), ms);
	};
};

export { debounce };