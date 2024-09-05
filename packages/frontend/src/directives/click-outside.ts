import { DirectiveOptions } from "vue/types/options";

interface ClickOutsideElement extends HTMLElement {
  clickOutsideEvent?: (event: MouseEvent | TouchEvent) => void;
}

const clickOutsideDirective: DirectiveOptions = {
  bind(el: ClickOutsideElement, binding, vnode) {
    el.clickOutsideEvent = function(event: MouseEvent | TouchEvent) {
      // Check if the click was outside the element and its children
      if (!(el === event.target || el.contains(event.target as Node))) {
        // Call the method provided in the directive's value
        const callback = binding.value as (
          event: MouseEvent | TouchEvent
        ) => void;
        if (typeof callback === "function") {
          callback(event);
        }
      }
    };
    document.body.addEventListener("click", el.clickOutsideEvent);
    document.body.addEventListener("touchstart", el.clickOutsideEvent);
  },
  unbind(el: ClickOutsideElement) {
    document.body.removeEventListener("click", el.clickOutsideEvent!);
    document.body.removeEventListener("touchstart", el.clickOutsideEvent!);
  },
};

export default clickOutsideDirective;
