import axios from "axios";

const httpClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? "/api",
  headers: { "Content-Type": "application/json" },
});


httpClient.interceptors.request.use((config) => {
  const token = localStorage.getItem("hms_token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

httpClient.interceptors.response.use(
  (response) => response,
  (error) => {
    const isLoginRequest = error.config?.url?.includes("/account/login");

    if (error.response?.status === 401 && !isLoginRequest) {
      localStorage.removeItem("hms_token");
      localStorage.removeItem("hms_user");
      window.location.href = "/login";
    }
    return Promise.reject(error);
  }
);

export default httpClient;