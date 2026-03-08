import React, { useState } from 'react';
import { Layout, Menu, Button, theme, Avatar, Dropdown } from 'antd';
import {
    MenuFoldOutlined,
    MenuUnfoldOutlined,
    DesktopOutlined,
    RocketOutlined,
    CalendarOutlined,
    HistoryOutlined,
    LogoutOutlined,
    UserOutlined,
} from '@ant-design/icons';
import { useNavigate, useLocation, Outlet } from 'react-router-dom';
import { useAuthStore } from '../store/authStore';

const { Header, Sider, Content } = Layout;

const DashboardLayout: React.FC = () => {
    const [collapsed, setCollapsed] = useState(false);
    const navigate = useNavigate();
    const location = useLocation();
    const { username, logout } = useAuthStore();

    const {
        token: { colorBgContainer, borderRadiusLG },
    } = theme.useToken();

    const handleLogout = () => {
        logout();
        navigate('/login');
    };

    const userMenuItems = [
        {
            key: 'logout',
            icon: <LogoutOutlined />,
            label: 'Đăng xuất',
            onClick: handleLogout,
        },
    ];

    return (
        <Layout className="min-h-screen">
            <Sider
                trigger={null}
                collapsible
                collapsed={collapsed}
                className="shadow-xl z-20"
                theme="dark"
            >
                <div className="flex items-center justify-center h-16 bg-slate-900 border-b border-slate-800 overflow-hidden px-4">
                    <div className="flex items-center gap-3">
                        <RocketOutlined className="text-2xl text-primary" />
                        {!collapsed && <span className="text-white font-bold text-lg tracking-tight">MCenter</span>}
                    </div>
                </div>
                <Menu
                    theme="dark"
                    mode="inline"
                    selectedKeys={[location.pathname]}
                    onClick={({ key }) => navigate(key)}
                    className="mt-2"
                    items={[
                        {
                            key: '/',
                            icon: <DesktopOutlined />,
                            label: 'Máy chủ',
                        },
                        {
                            key: '/workflows',
                            icon: <RocketOutlined />,
                            label: 'Workflows',
                        },
                        {
                            key: '/schedules',
                            icon: <CalendarOutlined />,
                            label: 'Lịch chạy',
                        },
                        {
                            key: '/executions',
                            icon: <HistoryOutlined />,
                            label: 'Lịch sử thực thi',
                        },
                    ]}
                />
            </Sider>
            <Layout>
                <Header
                    className="px-6 flex items-center justify-between shadow-md z-10 sticky top-0"
                    style={{ background: colorBgContainer, height: 64, padding: '0 24px' }}
                >
                    <Button
                        type="text"
                        icon={collapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
                        onClick={() => setCollapsed(!collapsed)}
                        className="text-lg w-10 h-10 flex items-center justify-center"
                    />

                    <div className="flex items-center gap-4">
                        <Dropdown menu={{ items: userMenuItems }} trigger={['click']}>
                            <div className="flex items-center gap-2 cursor-pointer hover:bg-slate-50 p-2 rounded-lg transition-colors">
                                <Avatar icon={<UserOutlined />} className="bg-primary" />
                                <span className="font-medium text-slate-700 hidden sm:inline">{username}</span>
                            </div>
                        </Dropdown>
                    </div>
                </Header>
                <Content
                    className="m-6 p-6 min-h-[280px] bg-slate-50 overflow-auto"
                    style={{
                        borderRadius: borderRadiusLG,
                    }}
                >
                    <Outlet />
                </Content>
            </Layout>
        </Layout>
    );
};

export default DashboardLayout;
