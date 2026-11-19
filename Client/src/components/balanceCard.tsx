import { formatDate } from "../utils/format";

const HOURS_PER_DAY = 7.4;

type Props = {
    title: string;
    hours: number;
    useBy: Date;
    lineColor: string;
};

export function BalanceCard({ title, hours, useBy, lineColor }: Props) {
    const days = Math.floor(hours / HOURS_PER_DAY);

    return (
        <div className={"bg-white border border-line rounded p-6 border-b-4 " + lineColor}>
            <h2 className="text-xl mb-2">{title}</h2>
            <p className="text-slate">Available now</p>

            <p className="mt-1">
                <span className="text-4xl font-bold">{hours.toFixed(2)} h</span>
                <span className="text-xl text-slate ml-3">≈ {days} days</span>
            </p>

            <p className="mt-3 text-sm text-slate">Use by: {formatDate(useBy)}</p>
        </div>
    );
}