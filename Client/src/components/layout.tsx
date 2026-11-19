import { NavLink, Outlet, useNavigate } from "react-router";
import { useAuth } from "./auth";
import logo from "../assets/logo.svg";

export type NavItem = {
    to: string;
    label: string;
};

type Props = {
    links: NavItem[];
};

export function Layout({ links }: Props) {
    const navigate = useNavigate();
    const { user, logout } = useAuth();

    async function handleLogout() {
        await logout();
        navigate("/login");
    }

    return (
        <div>
            <nav className="flex items-end gap-8 px-8 pt-6 border-b-4 border-navy">
                <img src={logo} alt="Business Esbjerg" className="h-10 mb-3 mr-8" />
                {links.map(link => (
                    <NavLink
                        key={link.to}
                        to={link.to}
                        end
                        className={({ isActive }) =>
                            "pb-2 font-semibold " +
                            (isActive ? "text-orange border-b-4 border-orange -mb-1" : "hover:text-orange")
                        }
                    >
                        {link.label}
                    </NavLink>
                ))}

                <span className="ml-auto pb-2 text-slate">{user?.email}</span>
                <button onClick={handleLogout} className="pb-2 font-semibold hover:text-orange">
                    Log out
                </button>
            </nav>

            <main className="p-8">
                <Outlet />
            </main>
        </div>
    );
}