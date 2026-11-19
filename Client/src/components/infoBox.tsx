import type { ReactNode } from "react";

type Props = {
    title: string;
    children?: ReactNode;
};

export function InfoBox({ title, children }: Props) {
    return (
        <div className="bg-white border border-line rounded p-6">
            <h2 className="text-sm font-semibold uppercase tracking-wide text-slate mb-3">{title}</h2>
            {children}
        </div>
    );
}
