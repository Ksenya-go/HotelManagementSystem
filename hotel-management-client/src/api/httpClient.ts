import axios from "axios";

const httpClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? "/api",
  headers: { "Content-Type": "application/json" },
  withCredentials: true, 
});

httpClient.interceptors.response.use(
  (response) => response,
  (error) => {
    const url: string = error.config?.url ?? "";
   
    const isAuthProbe = url.includes("/account/login") || url.includes("/account/me");

    if (error.response?.status === 401 && !isAuthProbe) {
      window.location.href = "/login";
    }
    return Promise.reject(error);
  }
);

export default httpClient;