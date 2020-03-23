export default function isInIframe() {
  return window.parent !== window;
}
