import { useEffect, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";

export interface Flash {
  type: "success" | "danger";
  text: string;
}

export function useFlash() {
  const location = useLocation();
  const navigate = useNavigate();
  const incoming = (location.state as { flash?: Flash } | null)?.flash ?? null;
  const [flash, setFlash] = useState<Flash | null>(incoming);

  
  useEffect(() => {
    if (incoming) {
      navigate(location.pathname + location.search, { replace: true, state: null });
    }
    
  }, []);

  return [flash, setFlash] as const;
}