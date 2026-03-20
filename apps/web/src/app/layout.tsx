import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import "@/app/globals.css";
import { AppProviders } from "@/components/providers/app-providers";

const sans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"]
});

const mono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"]
});

export const metadata: Metadata = {
  title: "FintechShield Security Console",
  description: "Secure authentication, session defense and anomaly response for fintech workloads."
};

export default function RootLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="en">
      <body className={`${sans.variable} ${mono.variable}`}>
        <AppProviders>{children}</AppProviders>
      </body>
    </html>
  );
}
