import { serve } from "bun";
import index from "./index.html";

// Adressen på .NET API'et (se launchSettings.json)
const apiUrl = "http://localhost:5159";

const server = serve({
  routes: {
    // Send alle /api-kald videre til .NET API'et, så login-cookien virker
    "/api/*": (req) => {
      const url = new URL(req.url);
      return fetch(new Request(apiUrl + url.pathname + url.search, req));
    },

    // Serve index.html for all routes
    "/*": index,
  },

  development: process.env.NODE_ENV !== "production" && {
    hmr: true,
    console: true,
  },
});

console.log(`🚀 Server running at ${server.url}`);
