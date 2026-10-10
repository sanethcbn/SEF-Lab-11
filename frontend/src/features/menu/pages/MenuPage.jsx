
import { useState } from "react";
import MenuList from "../components/MenuList";
import { useDebounce } from "../hooks/useDebounce";
import { useFetch } from "../hooks/useFetch";

function MenuPage() {
  const [query, setQuery] = useState("");

  const debounced = useDebounce(query, 400);

  // Read API base URL from environment variable
  const API = import.meta.env.VITE_API_URL;

  // Use backend API if configured, otherwise local JSON
  const menuUrl =
    API && API !== "https://example.invalid"
      ? `${API.replace(/\/$/, "")}/api/menu`
      : "/menu.json";

  // Fetch menu data
  const {
    data: dishes,
    isLoading,
    error,
  } = useFetch(menuUrl);

  // Loading State
  if (isLoading) {
    return <p>Loading menu...</p>;
  }

  // Error State
  if (error) {
    return <p>Could not load menu: {error}</p>;
  }

  // Success State
  const filtered = (dishes ?? []).filter((dish) =>
    dish.name.toLowerCase().includes(debounced.toLowerCase())
  );

  return (
    <div>
      <h1>Campus Eats Dashboard</h1>
      <h3>Menu</h3>

      <input
        type="text"
        value={query}
        onChange={(e) => setQuery(e.target.value)}
        placeholder="Search dishes..."
      />

      <MenuList dishes={filtered} />
    </div>
  );
}

export default MenuPage;
