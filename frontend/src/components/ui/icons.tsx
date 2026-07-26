import type { SVGProps } from 'react';

type IconProps = SVGProps<SVGSVGElement> & { size?: number };

const svgBase = (size: number): SVGProps<SVGSVGElement> => ({
  width: size,
  height: size,
  viewBox: '0 0 24 24',
  fill: 'none',
  stroke: 'currentColor',
  strokeLinecap: 'round',
  strokeLinejoin: 'round',
});

export const CheckIcon = ({ size = 20, ...rest }: IconProps) => (
  <svg {...svgBase(size)} strokeWidth={3.4} {...rest}>
    <path d="M20 6 9 17l-5-5" />
  </svg>
);

export const ChevronLeftIcon = ({ size = 20, ...rest }: IconProps) => (
  <svg {...svgBase(size)} strokeWidth={3} {...rest}>
    <path d="m15 18-6-6 6-6" />
  </svg>
);

export const ChevronRightIcon = ({ size = 20, ...rest }: IconProps) => (
  <svg {...svgBase(size)} strokeWidth={3} {...rest}>
    <path d="m9 18 6-6-6-6" />
  </svg>
);

export const MoreVerticalIcon = ({ size = 20, ...rest }: IconProps) => (
  <svg {...svgBase(size)} strokeWidth={2.6} {...rest}>
    <circle cx="12" cy="5" r="1" />
    <circle cx="12" cy="12" r="1" />
    <circle cx="12" cy="19" r="1" />
  </svg>
);

export const PlusIcon = ({ size = 14, ...rest }: IconProps) => (
  <svg {...svgBase(size)} strokeWidth={3} {...rest}>
    <path d="M5 12h14M12 5v14" />
  </svg>
);
