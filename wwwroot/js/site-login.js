(function () {
  // run only on Identity login pages
  const isLogin = location.pathname.toLowerCase().includes("/identity/account/login");
  if (!isLogin) return;

  // find password inputs
  const pw = document.querySelector('input[type="password"][id*="Password"]');
  if (!pw) return;

  // build the toggle
  const wrapper = document.createElement("div");
  wrapper.className = "form-check mt-2";

  const cb = document.createElement("input");
  cb.type = "checkbox";
  cb.className = "form-check-input";
  cb.id = "toggleShowPassword";

  const label = document.createElement("label");
  label.className = "form-check-label";
  label.htmlFor = "toggleShowPassword";
  label.textContent = "Show password";

  wrapper.appendChild(cb);
  wrapper.appendChild(label);

  // insert right after the password input
  pw.parentElement.appendChild(wrapper);

  cb.addEventListener("change", () => {
    pw.type = cb.checked ? "text" : "password";
  });
})();
