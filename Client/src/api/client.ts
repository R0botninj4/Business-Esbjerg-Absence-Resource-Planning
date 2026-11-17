import { Api } from "./Api";

// Tom baseUrl, så kaldene går til samme adresse som siden (og dermed gennem proxyen i index.ts)
export const api = new Api({ baseUrl: "" });