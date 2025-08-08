declare module 'lucide-react' {
  import { ComponentType, SVGProps } from 'react';

  export interface IconProps extends SVGProps<SVGSVGElement> {
    size?: number | string;
    color?: string;
    strokeWidth?: number;
  }

  export const Search: ComponentType<IconProps>;
  export const Plus: ComponentType<IconProps>;
  export const Edit: ComponentType<IconProps>;
  export const Trash2: ComponentType<IconProps>;
  export const Phone: ComponentType<IconProps>;
  export const Mail: ComponentType<IconProps>;
  export const User: ComponentType<IconProps>;
  export const Users: ComponentType<IconProps>;
  export const Calendar: ComponentType<IconProps>;
  export const Filter: ComponentType<IconProps>;
  export const Download: ComponentType<IconProps>;
  export const RotateCw: ComponentType<IconProps>;
  export const Eye: ComponentType<IconProps>;
  export const GraduationCap: ComponentType<IconProps>;
  export const Clock: ComponentType<IconProps>;
  export const DollarSign: ComponentType<IconProps>;
  export const ArrowRight: ComponentType<IconProps>;
  export const Save: ComponentType<IconProps>;
  export const UserPlus: ComponentType<IconProps>;
  export const UserMinus: ComponentType<IconProps>;
  export const CheckCircle: ComponentType<IconProps>;
  export const XCircle: ComponentType<IconProps>;
} 