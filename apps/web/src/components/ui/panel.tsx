import { cn } from "@/lib/utils/cn";

export function Panel({
  className,
  children
}: Readonly<{ className?: string; children: React.ReactNode }>) {
  return <section className={cn("glass rounded-[28px] p-6", className)}>{children}</section>;
}
