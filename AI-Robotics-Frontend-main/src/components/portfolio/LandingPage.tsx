import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import {
  Brain,
  Bot,
  Code,
  Cpu,
  ArrowRight,
  Phone,
  Mail,
  MapPin,
  Users,
  Award,
  Star,
  Monitor,
  Coffee,
  Calendar,
  Zap,
  Rocket,
  Sparkles,
  CircuitBoard,
  Wifi,
  Database,
  Smartphone,
  Globe,
  Shield,
  Target,
  TrendingUp,
  Lightbulb,
  Cog,
  Atom,
  Binary,
  Microwave as Microchip,
  BookOpen,
  Facebook,
  Instagram,
} from "lucide-react";

const LandingPage: React.FC = () => {
  const [mousePosition, setMousePosition] = useState({ x: 0, y: 0 });
  const [isVisible, setIsVisible] = useState(false);

  useEffect(() => {
    setIsVisible(true);

    const handleMouseMove = (e: MouseEvent) => {
      setMousePosition({ x: e.clientX, y: e.clientY });
    };

    window.addEventListener("mousemove", handleMouseMove);
    return () => window.removeEventListener("mousemove", handleMouseMove);
  }, []);

  // Floating particles component
  const FloatingParticles = () => (
    <div className="absolute inset-0 overflow-hidden pointer-events-none">
      {[...Array(20)].map((_, i) => (
        <div
          key={i}
          className="particle"
          style={{
            left: `${Math.random() * 100}%`,
            top: `${Math.random() * 100}%`,
            animationDelay: `${Math.random() * 6}s`,
            animationDuration: `${4 + Math.random() * 4}s`,
          }}
        />
      ))}
    </div>
  );

  // Animated robot component
  const AnimatedRobot = ({ className = "", size = "w-8 h-8" }) => (
    <div className={`${className} robot-icon`}>
      <Bot className={`${size} text-primary-500`} />
    </div>
  );

  // Circuit pattern component
  const CircuitPattern = () => (
    <svg
      className="absolute inset-0 w-full h-full opacity-10"
      viewBox="0 0 100 100"
    >
      <defs>
        <pattern
          id="circuit"
          x="0"
          y="0"
          width="20"
          height="20"
          patternUnits="userSpaceOnUse"
        >
          <circle cx="10" cy="10" r="1" fill="#0ea5e9" />
          <path
            d="M10 0v20M0 10h20"
            stroke="#0ea5e9"
            strokeWidth="0.5"
            className="circuit-line"
          />
        </pattern>
      </defs>
      <rect width="100%" height="100%" fill="url(#circuit)" />
    </svg>
  );

  return (
    <div className="min-h-screen bg-gradient-to-br from-secondary-900 via-secondary-800 to-primary-900 text-white rtl overflow-hidden relative">
      <FloatingParticles />
      <CircuitPattern />

      {/* Animated background elements */}
      <div className="absolute inset-0 cyber-bg opacity-20"></div>

      {/* Header */}
      <header className="relative z-50 bg-secondary-900/80 backdrop-blur-lg shadow-2xl sticky top-0 border-b border-primary-500/20">
        <nav className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <div className="flex justify-between items-center">
            <div
              className={`flex items-center space-x-4 rtl:space-x-reverse transition-all duration-700 ${
                isVisible ? "animate-slide-right" : "opacity-0"
              }`}
            >
              <div className="relative">
                <div className="w-12 h-12 bg-gradient-to-br from-primary-400 to-accent-500 rounded-xl flex items-center justify-center glow-blue animate-pulse-glow">
                  <Brain className="w-7 h-7 text-white animate-float" />
                </div>
                <div className="absolute -top-1 -right-1 w-4 h-4 bg-electric-400 rounded-full animate-bounce-gentle"></div>
              </div>
              <div>
                <h1 className="text-xl font-bold bg-gradient-to-r from-primary-300 to-accent-400 bg-clip-text text-transparent neon-text">
                  شركة الذكاء الاصطناعي
                </h1>
                <p className="text-sm text-electric-300 tech-font">
                  والروبوتات المتقدمة
                </p>
              </div>
            </div>

            <div
              className={`transition-all duration-700 delay-300 ${
                isVisible ? "animate-slide-left" : "opacity-0"
              }`}
            >
              <Link
                to="/login"
                className="group relative bg-gradient-to-r from-primary-500 to-accent-500 text-white px-8 py-3 rounded-xl hover:from-primary-400 hover:to-accent-400 transition-all duration-300 flex items-center space-x-2 rtl:space-x-reverse glow-blue hover:glow-purple transform hover:scale-105"
              >
                <span className="font-medium">دخول النظام</span>
                <ArrowRight className="w-5 h-5 group-hover:translate-x-1 transition-transform" />
                <div className="absolute inset-0 bg-gradient-to-r from-primary-400 to-accent-400 rounded-xl opacity-0 group-hover:opacity-20 transition-opacity"></div>
              </Link>
            </div>
          </div>
        </nav>
      </header>

      {/* Hero Section */}
      <section className="relative py-20 overflow-hidden">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid lg:grid-cols-2 gap-12 items-center">
            <div
              className={`transition-all duration-1000 ${
                isVisible ? "animate-slide-up" : "opacity-0 translate-y-10"
              }`}
            >
              <div className="mb-6">
                <span className="inline-block bg-gradient-to-r from-electric-400 to-primary-400 text-secondary-900 px-4 py-2 rounded-full text-sm font-medium animate-bounce-gentle">
                  🚀 مستقبل التعليم التقني
                </span>
              </div>

              <h1 className="text-4xl md:text-5xl lg:text-6xl font-bold mb-6 leading-tight">
                <span className="block holographic animate-neon-pulse">
                  مستقبل التعليم في
                </span>
                <span className="block bg-gradient-to-r from-primary-300 via-accent-400 to-electric-300 bg-clip-text text-transparent">
                  الذكاء الاصطناعي
                </span>
                <span className="block text-electric-300">والروبوتات</span>
              </h1>

              <p className="text-xl text-gray-300 mb-8 leading-relaxed">
                نقدم تعليماً متميزاً في مجالات الذكاء الاصطناعي والروبوتات
                والبرمجة للأطفال والشباب من سن 4 سنوات حتى 18+ سنة مع أحدث
                التقنيات العالمية
              </p>

              <div className="flex flex-col sm:flex-row gap-4 mb-8">
                {/* قم بإزالة أو إخفاء أي عنصر فيه to="/register" أو نص التسجيل الآن أو زر التسجيل */}
                <Link
                  to="/register"
                  className="group relative bg-gradient-to-r from-primary-500 via-accent-500 to-electric-500 text-white px-8 py-4 rounded-xl hover:from-primary-400 hover:via-accent-400 hover:to-electric-400 transition-all duration-300 flex items-center justify-center space-x-2 rtl:space-x-reverse text-lg font-medium glow-blue hover:glow-purple transform hover:scale-105"
                >
                  <Rocket className="w-6 h-6 group-hover:animate-bounce-gentle" />
                  <span>سجل الآن</span>
                  <ArrowRight className="w-5 h-5 group-hover:translate-x-1 transition-transform" />
                </Link>

                <button
                  onClick={() => {
                    // Create video modal
                    const modal = document.createElement("div");
                    modal.className =
                      "fixed inset-0 bg-black bg-opacity-75 flex items-center justify-center z-50";
                    modal.innerHTML = `
                      <div class="relative max-w-4xl w-full mx-4">
                        <button class="absolute -top-10 right-0 text-white text-2xl hover:text-gray-300" onclick="this.parentElement.parentElement.remove()">
                          ✕
                        </button>
                        <video controls class="w-full rounded-lg">
                          <source src="/فيديو كريم لتعليم البرمجه.mp4" type="video/mp4">
                          متصفحك لا يدعم تشغيل الفيديو
                        </video>
                      </div>
                    `;
                    document.body.appendChild(modal);
                    modal.addEventListener("click", (e) => {
                      if (e.target === modal) modal.remove();
                    });
                  }}
                  className="group border-2 border-accent-400 text-accent-300 px-8 py-4 rounded-xl hover:bg-accent-400/10 transition-all duration-300 flex items-center justify-center text-lg font-medium hover:border-electric-400 hover:text-electric-300"
                >
                  <Sparkles className="w-5 h-5 ml-2 group-hover:animate-wiggle" />
                  مشاهدة العرض
                </button>
              </div>

              {/* Tech badges */}
              <div className="flex flex-wrap gap-3">
                {[
                  {
                    icon: Brain,
                    label: "AI",
                    color: "from-primary-500 to-primary-600",
                  },
                  {
                    icon: Bot,
                    label: "Robotics",
                    color: "from-accent-500 to-accent-600",
                  },
                  {
                    icon: Code,
                    label: "Programming",
                    color: "from-electric-500 to-electric-600",
                  },
                  {
                    icon: Cpu,
                    label: "IoT",
                    color: "from-warning-500 to-warning-600",
                  },
                ].map((tech, index) => (
                  <div
                    key={index}
                    className={`flex items-center space-x-2 rtl:space-x-reverse bg-gradient-to-r ${tech.color} px-4 py-2 rounded-lg text-white text-sm font-medium animate-fade-in glow-blue`}
                    style={{ animationDelay: `${index * 0.2}s` }}
                  >
                    <tech.icon className="w-4 h-4 animate-float" />
                    <span>{tech.label}</span>
                  </div>
                ))}
              </div>
            </div>

            {/* Interactive 3D-like display */}
            <div
              className={`relative transition-all duration-1000 delay-500 ${
                isVisible ? "animate-slide-left" : "opacity-0 translate-x-10"
              }`}
            >
              <div className="relative bg-gradient-to-br from-secondary-800/50 to-secondary-900/50 backdrop-blur-lg rounded-3xl shadow-2xl p-8 border border-primary-500/20 glow-blue">
                <div className="absolute inset-0 bg-gradient-to-br from-primary-500/10 to-accent-500/10 rounded-3xl"></div>

                {/* Animated grid */}
                <div className="grid grid-cols-2 gap-6 relative z-10">
                  {[
                    {
                      icon: Bot,
                      title: "الروبوتات",
                      desc: "تصميم وبرمجة الروبوتات",
                      color: "from-primary-400 to-primary-600",
                      delay: "0s",
                    },
                    {
                      icon: Brain,
                      title: "الذكاء الاصطناعي",
                      desc: "تطبيقات الذكاء الاصطناعي",
                      color: "from-accent-400 to-accent-600",
                      delay: "0.2s",
                    },
                    {
                      icon: Code,
                      title: "البرمجة",
                      desc: "Python, C++, JavaScript",
                      color: "from-electric-400 to-electric-600",
                      delay: "0.4s",
                    },
                    {
                      icon: Cpu,
                      title: "الميكاترونيكس",
                      desc: "الهندسة الإلكترونية",
                      color: "from-warning-400 to-warning-600",
                      delay: "0.6s",
                    },
                  ].map((item, index) => (
                    <div
                      key={index}
                      className="group bg-gradient-to-br from-secondary-700/50 to-secondary-800/50 backdrop-blur-sm p-6 rounded-2xl text-center hover:from-secondary-600/50 hover:to-secondary-700/50 transition-all duration-300 border border-primary-500/10 hover:border-primary-400/30 cursor-pointer transform hover:scale-105 animate-fade-in glow-blue"
                      style={{ animationDelay: item.delay }}
                    >
                      <div
                        className={`w-16 h-16 bg-gradient-to-br ${item.color} rounded-2xl flex items-center justify-center mx-auto mb-4 group-hover:animate-bounce-gentle glow-blue`}
                      >
                        <item.icon className="w-8 h-8 text-white animate-float" />
                      </div>
                      <h3 className="font-bold text-white mb-2 group-hover:text-primary-300 transition-colors">
                        {item.title}
                      </h3>
                      <p className="text-sm text-gray-400 group-hover:text-gray-300 transition-colors">
                        {item.desc}
                      </p>

                      {/* Animated circuit lines */}
                      <div className="absolute inset-0 opacity-0 group-hover:opacity-20 transition-opacity">
                        <svg className="w-full h-full">
                          <path
                            d="M0,50 Q50,0 100,50 T200,50"
                            stroke="#0ea5e9"
                            strokeWidth="1"
                            fill="none"
                            className="circuit-line"
                          />
                        </svg>
                      </div>
                    </div>
                  ))}
                </div>

                {/* Floating robots */}
                <AnimatedRobot
                  className="absolute -top-4 -right-4"
                  size="w-6 h-6"
                />
                <AnimatedRobot
                  className="absolute -bottom-4 -left-4"
                  size="w-6 h-6"
                />

                {/* Holographic effect overlay */}
                <div className="absolute inset-0 bg-gradient-to-r from-transparent via-primary-400/5 to-transparent animate-pulse rounded-3xl"></div>
              </div>

              {/* Orbiting elements */}
              <div className="absolute -inset-4">
                {[Zap, Atom, Binary, Microchip].map((Icon, index) => (
                  <div
                    key={index}
                    className="absolute w-8 h-8 text-primary-400 animate-rotate-slow"
                    style={{
                      top: `${25 + Math.sin((index * Math.PI) / 2) * 40}%`,
                      left: `${25 + Math.cos((index * Math.PI) / 2) * 40}%`,
                      animationDelay: `${index * 0.5}s`,
                      animationDuration: `${8 + index}s`,
                    }}
                  >
                    <Icon className="w-full h-full animate-float" />
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Animated Stats Section */}
      <section className="py-16 relative">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid grid-cols-2 lg:grid-cols-4 gap-8">
            {[
              {
                number: "500+",
                label: "طالب متخرج",
                icon: Users,
                color: "text-primary-400",
              },
              {
                number: "50+",
                label: "كورس تدريبي",
                icon: BookOpen,
                color: "text-electric-400",
              },
              {
                number: "15+",
                label: "مدرب خبير",
                icon: Award,
                color: "text-accent-400",
              },
              {
                number: "2",
                label: "فرع متاح",
                icon: MapPin,
                color: "text-warning-400",
              },
            ].map((stat, index) => (
              <div
                key={index}
                className={`text-center group animate-fade-in`}
                style={{ animationDelay: `${index * 0.2}s` }}
              >
                <div className="relative mb-4">
                  <div
                    className={`w-16 h-16 mx-auto bg-gradient-to-br from-secondary-700 to-secondary-800 rounded-2xl flex items-center justify-center group-hover:from-secondary-600 group-hover:to-secondary-700 transition-all duration-300 glow-blue`}
                  >
                    <stat.icon
                      className={`w-8 h-8 ${stat.color} group-hover:animate-bounce-gentle`}
                    />
                  </div>
                  <div className="absolute -inset-2 bg-gradient-to-r from-primary-500/20 to-accent-500/20 rounded-2xl opacity-0 group-hover:opacity-100 transition-opacity blur-sm"></div>
                </div>
                <div
                  className={`text-4xl font-bold ${stat.color} mb-2 tech-font group-hover:animate-pulse-glow`}
                >
                  {stat.number}
                </div>
                <div className="text-gray-400 group-hover:text-gray-300 transition-colors">
                  {stat.label}
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Enhanced Courses Section */}
      <section id="courses" className="py-20 relative">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-16">
            <div className="inline-block bg-gradient-to-r from-primary-500/20 to-accent-500/20 backdrop-blur-sm px-6 py-3 rounded-full mb-6 border border-primary-500/30">
              <span className="text-primary-300 font-medium flex items-center space-x-2 rtl:space-x-reverse">
                <Sparkles className="w-5 h-5 animate-wiggle" />
                <span>برامجنا التعليمية</span>
                <Sparkles className="w-5 h-5 animate-wiggle" />
              </span>
            </div>
            <h2 className="text-5xl font-bold text-white mb-4 holographic">
              برامجنا التعليمية
            </h2>
            <p className="text-xl text-gray-300">
              تعليم متدرج يناسب جميع الأعمار والمستويات مع أحدث التقنيات
            </p>
          </div>

          <div className="grid lg:grid-cols-3 gap-8">
            {/* Ages 4-6 */}
            <div className="group relative bg-gradient-to-br from-secondary-800/50 to-secondary-900/50 backdrop-blur-lg rounded-3xl shadow-2xl p-8 hover:from-secondary-700/50 hover:to-secondary-800/50 transition-all duration-500 border border-primary-500/20 hover:border-primary-400/40 transform hover:scale-105 glow-blue">
              <div className="absolute inset-0 bg-gradient-to-br from-primary-500/5 to-accent-500/5 rounded-3xl opacity-0 group-hover:opacity-100 transition-opacity"></div>

              <div className="relative z-10">
                <div className="flex items-center justify-between mb-6">
                  <div className="w-20 h-20 bg-gradient-to-br from-primary-400 to-primary-600 rounded-2xl flex items-center justify-center group-hover:animate-bounce-gentle glow-blue">
                    <Star className="w-10 h-10 text-white animate-float" />
                  </div>
                  <AnimatedRobot size="w-8 h-8" />
                </div>

                <h3 className="text-2xl font-bold text-white mb-2 group-hover:text-primary-300 transition-colors">
                  المرحلة التأسيسية
                </h3>
                <p className="text-primary-300 mb-4 font-medium">
                  للأطفال من سن 4-6 سنوات
                </p>

                <ul className="space-y-3 mb-6">
                  {[
                    "أساسيات الروبوتيكس والتفكير المنطقي",
                    "STEM Education المتقدم",
                    "أدوات تعليمية عالمية حديثة",
                  ].map((item, index) => (
                    <li
                      key={index}
                      className="flex items-center text-gray-300 group-hover:text-gray-200 transition-colors"
                    >
                      <div className="w-3 h-3 bg-gradient-to-r from-primary-400 to-accent-400 rounded-full ml-3 animate-pulse-glow"></div>
                      {item}
                    </li>
                  ))}
                </ul>

                <div className="text-3xl font-bold text-primary-400 mb-4 tech-font group-hover:animate-neon-pulse">
                  300 ج.م/شهر
                </div>

                <button className="w-full bg-gradient-to-r from-primary-500 to-primary-600 text-white py-3 rounded-xl hover:from-primary-400 hover:to-primary-500 transition-all duration-300 font-medium group-hover:glow-blue">
                  اشترك الآن
                </button>
              </div>
            </div>

            {/* Ages 7-12 - Featured */}
            <div className="group relative bg-gradient-to-br from-accent-800/50 to-accent-900/50 backdrop-blur-lg rounded-3xl shadow-2xl p-8 hover:from-accent-700/50 hover:to-accent-800/50 transition-all duration-500 border-2 border-accent-400/40 transform hover:scale-105 glow-purple">
              <div className="absolute inset-0 bg-gradient-to-br from-accent-500/10 to-electric-500/10 rounded-3xl"></div>

              {/* Featured badge */}
              <div className="absolute -top-4 left-1/2 transform -translate-x-1/2">
                <div className="bg-gradient-to-r from-accent-400 to-electric-400 text-white px-6 py-2 rounded-full text-sm font-bold animate-bounce-gentle">
                  ⭐ الأكثر شعبية
                </div>
              </div>

              <div className="relative z-10 pt-4">
                <div className="flex items-center justify-between mb-6">
                  <div className="w-20 h-20 bg-gradient-to-br from-accent-400 to-electric-500 rounded-2xl flex items-center justify-center group-hover:animate-bounce-gentle glow-purple">
                    <Users className="w-10 h-10 text-white animate-float" />
                  </div>
                  <AnimatedRobot size="w-8 h-8" />
                </div>

                <h3 className="text-2xl font-bold text-white mb-2 group-hover:text-accent-300 transition-colors">
                  المرحلة الاستكشافية
                </h3>
                <p className="text-accent-300 mb-4 font-medium">
                  للأطفال من سن 7-12 سنة
                </p>

                <ul className="space-y-3 mb-6">
                  {[
                    "الروبوتيكس والذكاء الاصطناعي المتقدم",
                    "أساسيات الكمبيوتر والبرمجة التفاعلية",
                    "الهندسة الكهربية والميكاترونيكس",
                  ].map((item, index) => (
                    <li
                      key={index}
                      className="flex items-center text-gray-300 group-hover:text-gray-200 transition-colors"
                    >
                      <div className="w-3 h-3 bg-gradient-to-r from-accent-400 to-electric-400 rounded-full ml-3 animate-pulse-glow"></div>
                      {item}
                    </li>
                  ))}
                </ul>

                <div className="text-3xl font-bold text-accent-400 mb-4 tech-font group-hover:animate-neon-pulse">
                  500 ج.م/شهر
                </div>

                <button className="w-full bg-gradient-to-r from-accent-500 to-electric-500 text-white py-3 rounded-xl hover:from-accent-400 hover:to-electric-400 transition-all duration-300 font-medium group-hover:glow-purple">
                  اشترك الآن
                </button>
              </div>
            </div>

            {/* Ages 13-17 */}
            <div className="group relative bg-gradient-to-br from-electric-800/50 to-electric-900/50 backdrop-blur-lg rounded-3xl shadow-2xl p-8 hover:from-electric-700/50 hover:to-electric-800/50 transition-all duration-500 border border-electric-500/20 hover:border-electric-400/40 transform hover:scale-105 glow-green">
              <div className="absolute inset-0 bg-gradient-to-br from-electric-500/5 to-warning-500/5 rounded-3xl opacity-0 group-hover:opacity-100 transition-opacity"></div>

              <div className="relative z-10">
                <div className="flex items-center justify-between mb-6">
                  <div className="w-20 h-20 bg-gradient-to-br from-electric-400 to-warning-500 rounded-2xl flex items-center justify-center group-hover:animate-bounce-gentle glow-green">
                    <Award className="w-10 h-10 text-white animate-float" />
                  </div>
                  <AnimatedRobot size="w-8 h-8" />
                </div>

                <h3 className="text-2xl font-bold text-white mb-2 group-hover:text-electric-300 transition-colors">
                  المرحلة المتقدمة
                </h3>
                <p className="text-electric-300 mb-4 font-medium">
                  للشباب من سن 13-17 سنة
                </p>

                <ul className="space-y-3 mb-6">
                  {[
                    "Python و C++ المتقدمة والاحترافية",
                    "مشاريع الروبوتات المعقدة والذكية",
                    "الذكاء الاصطناعي التطبيقي والعملي",
                  ].map((item, index) => (
                    <li
                      key={index}
                      className="flex items-center text-gray-300 group-hover:text-gray-200 transition-colors"
                    >
                      <div className="w-3 h-3 bg-gradient-to-r from-electric-400 to-warning-400 rounded-full ml-3 animate-pulse-glow"></div>
                      {item}
                    </li>
                  ))}
                </ul>

                <div className="text-3xl font-bold text-electric-400 mb-4 tech-font group-hover:animate-neon-pulse">
                  700 ج.م/شهر
                </div>

                <button className="w-full bg-gradient-to-r from-electric-500 to-warning-500 text-white py-3 rounded-xl hover:from-electric-400 hover:to-warning-400 transition-all duration-300 font-medium group-hover:glow-green">
                  اشترك الآن
                </button>
              </div>
            </div>
          </div>

          {/* Adult Specialization */}
          <div className="mt-20">
            <div className="text-center mb-12">
              <h3 className="text-4xl font-bold text-white mb-4 holographic">
                تخصصات البالغين (18+ سنة)
              </h3>
              <p className="text-xl text-gray-300">
                تخصصات احترافية متقدمة لسوق العمل
              </p>
            </div>

            <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
              {[
                {
                  name: "تطوير المواقع",
                  desc: "Web Development",
                  icon: Globe,
                  color: "from-primary-500 to-primary-600",
                },
                {
                  name: "تطبيقات الموبايل",
                  desc: "Mobile Apps",
                  icon: Smartphone,
                  color: "from-accent-500 to-accent-600",
                },
                {
                  name: "تطبيقات سطح المكتب",
                  desc: "Desktop Apps",
                  icon: Monitor,
                  color: "from-electric-500 to-electric-600",
                },
                {
                  name: "تحليل البيانات",
                  desc: "Data Analysis",
                  icon: Database,
                  color: "from-warning-500 to-warning-600",
                },
                {
                  name: "الروبوتات المتقدمة",
                  desc: "Advanced Robotics",
                  icon: Bot,
                  color: "from-error-500 to-error-600",
                },
                {
                  name: "الميكاترونيكس",
                  desc: "Mechatronics",
                  icon: Cog,
                  color: "from-secondary-500 to-secondary-600",
                },
              ].map((course, index) => (
                <div
                  key={index}
                  className={`group bg-gradient-to-br from-secondary-800/50 to-secondary-900/50 backdrop-blur-lg rounded-2xl shadow-xl p-6 hover:from-secondary-700/50 hover:to-secondary-800/50 transition-all duration-300 border border-primary-500/10 hover:border-primary-400/30 cursor-pointer transform hover:scale-105 animate-fade-in glow-blue`}
                  style={{ animationDelay: `${index * 0.1}s` }}
                >
                  <div className="flex items-center space-x-4 rtl:space-x-reverse">
                    <div
                      className={`w-12 h-12 bg-gradient-to-br ${course.color} rounded-xl flex items-center justify-center group-hover:animate-bounce-gentle glow-blue`}
                    >
                      <course.icon className="w-6 h-6 text-white animate-float" />
                    </div>
                    <div>
                      <h4 className="text-lg font-semibold text-white mb-1 group-hover:text-primary-300 transition-colors">
                        {course.name}
                      </h4>
                      <p className="text-gray-400 group-hover:text-gray-300 transition-colors tech-font">
                        {course.desc}
                      </p>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </section>

      {/* Enhanced Features Section */}
      <section className="py-20 relative">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-16">
            <h2 className="text-5xl font-bold text-white mb-4 holographic">
              مميزات أكاديميتنا
            </h2>
            <p className="text-xl text-gray-300">
              لماذا نحن الاختيار الأمثل للمستقبل التقني؟
            </p>
          </div>

          <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-8">
            {[
              {
                icon: Monitor,
                title: "مساحات عمل ذكية",
                desc: "مساحات عمل فردية ومشتركة مجهزة بأحدث التقنيات والذكاء الاصطناعي",
                color: "from-primary-500 to-primary-600",
              },
              {
                icon: Users,
                title: "مدربون خبراء",
                desc: "فريق من المهندسين المتخصصين والخبراء في كل مجال تقني",
                color: "from-accent-500 to-accent-600",
              },
              {
                icon: Calendar,
                title: "جدولة ذكية",
                desc: "نظام جدولة متطور ومرن يناسب جميع الطلاب والمواعيد",
                color: "from-electric-500 to-electric-600",
              },
              {
                icon: Award,
                title: "شهادات معتمدة دولياً",
                desc: "شهادات معتمدة عالمياً بعد إتمام كل مستوى تعليمي",
                color: "from-warning-500 to-warning-600",
              },
              {
                icon: Coffee,
                title: "كافيتريا تقنية",
                desc: "كافيتريا متكاملة مع تقنيات ذكية لراحة الطلاب",
                color: "from-error-500 to-error-600",
              },
              {
                icon: Bot,
                title: "مشاريع حقيقية",
                desc: "العمل على مشاريع عملية وحقيقية مع شركات التكنولوجيا",
                color: "from-secondary-500 to-secondary-600",
              },
            ].map((feature, index) => (
              <div
                key={index}
                className={`group text-center p-8 bg-gradient-to-br from-secondary-800/50 to-secondary-900/50 backdrop-blur-lg rounded-2xl hover:from-secondary-700/50 hover:to-secondary-800/50 transition-all duration-300 border border-primary-500/10 hover:border-primary-400/30 cursor-pointer transform hover:scale-105 animate-fade-in glow-blue`}
                style={{ animationDelay: `${index * 0.1}s` }}
              >
                <div
                  className={`w-20 h-20 bg-gradient-to-br ${feature.color} rounded-2xl flex items-center justify-center mx-auto mb-6 group-hover:animate-bounce-gentle glow-blue`}
                >
                  <feature.icon className="w-10 h-10 text-white animate-float" />
                </div>
                <h3 className="text-xl font-semibold text-white mb-4 group-hover:text-primary-300 transition-colors">
                  {feature.title}
                </h3>
                <p className="text-gray-400 group-hover:text-gray-300 transition-colors leading-relaxed">
                  {feature.desc}
                </p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Enhanced Branches Section */}
      <section className="py-20 relative">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-16">
            <h2 className="text-5xl font-bold text-white mb-4 holographic">
              فروعنا التقنية
            </h2>
            <p className="text-xl text-gray-300">
              نخدمك في موقعين استراتيجيين بأحدث التقنيات
            </p>
          </div>

          <div className="grid lg:grid-cols-2 gap-12">
            {/* Assiut Branch */}
            <div className="group bg-gradient-to-br from-secondary-800/50 to-secondary-900/50 backdrop-blur-lg rounded-3xl shadow-2xl p-8 border border-primary-500/20 hover:border-primary-400/40 transition-all duration-500 glow-blue">
              <div className="relative">
                <div className="flex items-center space-x-4 rtl:space-x-reverse mb-6">
                  <div className="w-16 h-16 bg-gradient-to-br from-primary-500 to-accent-500 rounded-2xl flex items-center justify-center glow-blue">
                    <MapPin className="w-8 h-8 text-white animate-float" />
                  </div>
                  <div>
                    <h3 className="text-2xl font-bold text-white group-hover:text-primary-300 transition-colors">
                      فرع أسيوط
                    </h3>
                    <p className="text-primary-300 font-medium">
                      الفرع الرئيسي المتكامل
                    </p>
                  </div>
                  <AnimatedRobot className="mr-auto" />
                </div>

                <div className="space-y-4 mb-6">
                  <div className="flex items-start space-x-3 rtl:space-x-reverse">
                    <MapPin className="w-5 h-5 text-primary-400 mt-1 animate-bounce-gentle" />
                    <div>
                      <p className="font-medium text-white">العنوان</p>
                      <p className="text-gray-300">شارع الجمهورية، أسيوط</p>
                    </div>
                  </div>
                  <div className="flex items-start space-x-3 rtl:space-x-reverse">
                    <Phone className="w-5 h-5 text-primary-400 mt-1 animate-bounce-gentle" />
                    <div>
                      <p className="font-medium text-white">الهاتف</p>
                      <p className="text-gray-300 tech-font" dir="ltr">
                        01040311505
                      </p>
                    </div>
                  </div>
                </div>

                <div className="border-t border-primary-500/20 pt-6">
                  <h4 className="font-semibold text-white mb-4 flex items-center">
                    <Sparkles className="w-5 h-5 text-primary-400 ml-2 animate-wiggle" />
                    الخدمات المتاحة:
                  </h4>
                  <div className="grid grid-cols-2 gap-3">
                    {[
                      "جميع الكورسات",
                      "مساحة العمل الفردية",
                      "مساحة العمل المشتركة",
                      "معامل مجهزة بالكامل",
                      "كافيتريا ذكية",
                      "نظام إدارة متكامل",
                    ].map((service, index) => (
                      <div
                        key={index}
                        className="flex items-center text-gray-300 group-hover:text-gray-200 transition-colors"
                      >
                        <div className="w-2 h-2 bg-gradient-to-r from-primary-400 to-accent-400 rounded-full ml-3 animate-pulse-glow"></div>
                        <span className="text-sm">{service}</span>
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            </div>

            {/* Abu Tig Branch */}
            <div className="group bg-gradient-to-br from-secondary-800/50 to-secondary-900/50 backdrop-blur-lg rounded-3xl shadow-2xl p-8 border border-electric-500/20 hover:border-electric-400/40 transition-all duration-500 glow-green">
              <div className="relative">
                <div className="flex items-center space-x-4 rtl:space-x-reverse mb-6">
                  <div className="w-16 h-16 bg-gradient-to-br from-electric-500 to-warning-500 rounded-2xl flex items-center justify-center glow-green">
                    <MapPin className="w-8 h-8 text-white animate-float" />
                  </div>
                  <div>
                    <h3 className="text-2xl font-bold text-white group-hover:text-electric-300 transition-colors">
                      فرع أبو تيج
                    </h3>
                    <p className="text-electric-300 font-medium">
                      التخصص في التعليم البرمجي
                    </p>
                  </div>
                  <AnimatedRobot className="mr-auto" />
                </div>

                <div className="space-y-4 mb-6">
                  <div className="flex items-start space-x-3 rtl:space-x-reverse">
                    <MapPin className="w-5 h-5 text-electric-400 mt-1 animate-bounce-gentle" />
                    <div>
                      <p className="font-medium text-white">العنوان</p>
                      <p className="text-gray-300">شارع الجامعة، أبو تيج</p>
                    </div>
                  </div>
                  <div className="flex items-start space-x-3 rtl:space-x-reverse">
                    <Phone className="w-5 h-5 text-electric-400 mt-1 animate-bounce-gentle" />
                    <div>
                      <p className="font-medium text-white">الهاتف</p>
                      <p className="text-gray-300 tech-font" dir="ltr">
                        01505227778
                      </p>
                    </div>
                  </div>
                </div>

                <div className="border-t border-electric-500/20 pt-6">
                  <h4 className="font-semibold text-white mb-4 flex items-center">
                    <Code className="w-5 h-5 text-electric-400 ml-2 animate-wiggle" />
                    الخدمات المتاحة:
                  </h4>
                  <div className="space-y-3">
                    {[
                      "جميع الكورسات البرمجية المتقدمة",
                      "تركيز على التعليم التقني",
                      "بيئة تعليمية مثالية ومتخصصة",
                      "مدربون متخصصون في البرمجة",
                    ].map((service, index) => (
                      <div
                        key={index}
                        className="flex items-center text-gray-300 group-hover:text-gray-200 transition-colors"
                      >
                        <div className="w-2 h-2 bg-gradient-to-r from-electric-400 to-warning-400 rounded-full ml-3 animate-pulse-glow"></div>
                        <span className="text-sm">{service}</span>
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Enhanced CTA Section */}
      <section className="py-20 relative overflow-hidden">
        <div className="absolute inset-0 bg-gradient-to-r from-primary-600/20 via-accent-600/20 to-electric-600/20"></div>
        <FloatingParticles />

        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 text-center relative z-10">
          <div className="mb-8">
            <h2 className="text-5xl font-bold mb-4 holographic animate-neon-pulse">
              ابدأ رحلتك معنا اليوم
            </h2>
            <p className="text-xl opacity-90 leading-relaxed">
              انضم إلى مجتمع المبدعين والمبتكرين في عالم التكنولوجيا المتقدمة
              <br />
              واكتشف إمكانياتك اللامحدودة في المستقبل الرقمي
            </p>
          </div>

          <div className="flex flex-col sm:flex-row gap-6 justify-center mb-12">
            <Link
              to="/register"
              className="group relative bg-gradient-to-r from-primary-500 via-accent-500 to-electric-500 text-white px-10 py-5 rounded-2xl hover:from-primary-400 hover:via-accent-400 hover:to-electric-400 transition-all duration-300 font-medium inline-flex items-center justify-center space-x-3 rtl:space-x-reverse text-lg glow-blue hover:glow-purple transform hover:scale-105"
            >
              <Rocket className="w-6 h-6 group-hover:animate-bounce-gentle" />
              <span>التسجيل الآن</span>
              <ArrowRight className="w-5 h-5 group-hover:translate-x-1 transition-transform" />
            </Link>

            <a
              href="tel:01040311505"
              className="group border-2 border-primary-400 text-primary-300 px-10 py-5 rounded-2xl hover:bg-primary-400/10 transition-all duration-300 font-medium inline-flex items-center justify-center space-x-3 rtl:space-x-reverse text-lg hover:border-accent-400 hover:text-accent-300"
            >
              <Phone className="w-6 h-6 group-hover:animate-wiggle" />
              <span>اتصل بنا</span>
            </a>
          </div>

          {/* Tech showcase */}
          <div className="flex justify-center items-center space-x-8 rtl:space-x-reverse">
            {[Brain, Bot, Code, Cpu, Zap].map((Icon, index) => (
              <div
                key={index}
                className="w-12 h-12 text-primary-400 animate-float"
                style={{ animationDelay: `${index * 0.2}s` }}
              >
                <Icon className="w-full h-full" />
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Enhanced Footer */}
      <footer className="bg-secondary-900/80 backdrop-blur-lg text-white py-12 border-t border-primary-500/20">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid md:grid-cols-3 gap-8">
            <div>
              <div className="flex items-center space-x-3 rtl:space-x-reverse mb-6">
                <div className="w-12 h-12 bg-gradient-to-br from-primary-500 to-accent-500 rounded-xl flex items-center justify-center glow-blue">
                  <Brain className="w-7 h-7 text-white animate-float" />
                </div>
                <div>
                  <h3 className="text-lg font-bold holographic">
                    شركة الذكاء الاصطناعي
                  </h3>
                  <p className="text-gray-400 text-sm tech-font">
                    والروبوتات المتقدمة
                  </p>
                </div>
              </div>
              <p className="text-gray-400 leading-relaxed">
                نقدم تعليماً متميزاً في مجالات التكنولوجيا الحديثة لإعداد جيل من
                المبدعين والمبتكرين في عالم المستقبل الرقمي.
              </p>
            </div>

            <div>
              <h4 className="text-lg font-semibold mb-4 flex items-center">
                <Sparkles className="w-5 h-5 text-primary-400 ml-2 animate-wiggle" />
                روابط سريعة
              </h4>
              <ul className="space-y-3">
                {[
                  { label: "الكورسات", href: "#courses" },
                  { label: "دخول النظام", href: "/login" },
                  { label: "التسجيل", href: "/register" },
                  { label: "من نحن", href: "#about" },
                ].map((link, index) => (
                  <li key={index}>
                    <Link
                      to={link.href}
                      className="text-gray-400 hover:text-primary-300 transition-colors flex items-center space-x-2 rtl:space-x-reverse group"
                    >
                      <ArrowRight className="w-4 h-4 group-hover:translate-x-1 transition-transform" />
                      <span>{link.label}</span>
                    </Link>
                  </li>
                ))}
              </ul>
            </div>

            <div>
              <h4 className="text-lg font-semibold mb-4 flex items-center">
                <Phone className="w-5 h-5 text-primary-400 ml-2 animate-bounce-gentle" />
                تواصل معنا
              </h4>
              <div className="space-y-4">
                {[
                  { icon: Phone, label: "01040311505", type: "tel" },
                  { icon: Phone, label: "01505227778", type: "tel" },
                  {
                    icon: Mail,
                    label: "info@ai-robotics-company.com",
                    type: "email",
                  },
                  {
                    icon: Facebook,
                    label: "فيسبوك",
                    type: "facebook",
                    href: "https://www.facebook.com/profile.php?id=61559788044093",
                  },
                  {
                    icon: Instagram,
                    label: "انستجرام",
                    type: "instagram",
                    href: "https://www.instagram.com/ai.robotics1?utm_source=ig_web_button_share_sheet&igsh=ZDNlZDc0MzIxNw==",
                  },
                  {
                    icon: MapPin,
                    label: "أسيوط - أبو تيج، مصر",
                    type: "address",
                  },
                ].map((contact, index) => (
                  <div
                    key={index}
                    className="flex items-center space-x-3 rtl:space-x-reverse group"
                  >
                    <contact.icon className="w-5 h-5 text-primary-400 group-hover:animate-bounce-gentle" />
                    {contact.href ? (
                      <a
                        href={contact.href}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="text-gray-400 group-hover:text-gray-300 transition-colors underline"
                      >
                        {contact.label}
                      </a>
                    ) : (
                      <span className="text-gray-400 group-hover:text-gray-300 transition-colors">
                        {contact.label}
                      </span>
                    )}
                  </div>
                ))}
              </div>
            </div>
          </div>

          <div className="border-t border-primary-500/20 mt-8 pt-8 text-center">
            <p className="text-gray-400 flex items-center justify-center space-x-2 rtl:space-x-reverse">
              <span>
                © 2025 شركة الذكاء الاصطناعي والروبوتات. جميع الحقوق محفوظة.
              </span>
              <Bot className="w-4 h-4 animate-robot-walk" />
            </p>
          </div>
        </div>
      </footer>
    </div>
  );
};

export default LandingPage;
