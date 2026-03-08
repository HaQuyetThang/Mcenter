import React, { useState } from 'react';
import { Form, Input, Button, Card, message, Typography } from 'antd';
import { UserOutlined, LockOutlined, RocketOutlined } from '@ant-design/icons';
import api from '../api/axios';
import { useAuthStore } from '../store/authStore';
import { useNavigate, Link } from 'react-router-dom';

const { Title, Text } = Typography;

const LoginPage: React.FC = () => {
    const [loading, setLoading] = useState(false);
    const login = useAuthStore((state) => state.login);
    const navigate = useNavigate();

    const onFinish = async (values: any) => {
        setLoading(true);
        try {
            const response = await api.post('/auth/login', values);
            login(response.data.token, response.data.username);
            message.success('Đăng nhập thành công!');
            navigate('/');
        } catch (error: any) {
            message.error(error.response?.data || 'Đăng nhập thất bại.');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="min-h-screen flex items-center justify-center bg-slate-900 overflow-hidden relative">
            {/* Decorative blobs */}
            <div className="absolute top-0 -left-4 w-72 h-72 bg-blue-500 rounded-full mix-blend-multiply filter blur-xl opacity-20 animate-blob"></div>
            <div className="absolute top-0 -right-4 w-72 h-72 bg-purple-500 rounded-full mix-blend-multiply filter blur-xl opacity-20 animate-blob animation-delay-2000"></div>
            <div className="absolute -bottom-8 left-20 w-72 h-72 bg-indigo-500 rounded-full mix-blend-multiply filter blur-xl opacity-20 animate-blob animation-delay-4000"></div>

            <Card className="w-full max-w-md shadow-2xl border-0 bg-slate-800 backdrop-blur-md bg-opacity-80">
                <div className="text-center mb-8">
                    <div className="inline-flex items-center justify-center w-16 h-16 rounded-2xl bg-primary text-white text-3xl mb-4 shadow-lg shadow-blue-500/50">
                        <RocketOutlined />
                    </div>
                    <Title level={2} className="!text-white !m-0 !mb-2">MCenter</Title>
                    <Text className="text-slate-400">RPA Orchestrator Management</Text>
                </div>

                <Form
                    name="login"
                    initialValues={{ remember: true }}
                    onFinish={onFinish}
                    layout="vertical"
                    size="large"
                >
                    <Form.Item
                        name="username"
                        rules={[{ required: true, message: 'Vui lòng nhập username!' }]}
                    >
                        <Input
                            prefix={<UserOutlined className="text-slate-400" />}
                            placeholder="Username"
                            className="!bg-slate-700 !border-slate-600 !text-white hover:!border-primary focus:!border-primary"
                        />
                    </Form.Item>

                    <Form.Item
                        name="password"
                        rules={[{ required: true, message: 'Vui lòng nhập mật khẩu!' }]}
                    >
                        <Input.Password
                            prefix={<LockOutlined className="text-slate-400" />}
                            placeholder="Mật khẩu"
                            className="!bg-slate-700 !border-slate-600 !text-white hover:!border-primary focus:!border-primary"
                        />
                    </Form.Item>

                    <Form.Item>
                        <Button
                            type="primary"
                            htmlType="submit"
                            className="w-full h-12 text-lg font-semibold shadow-lg shadow-blue-500/30"
                            loading={loading}
                        >
                            Đăng nhập
                        </Button>
                    </Form.Item>

                    <div className="text-center space-y-2">
                        <div>
                            <Text className="text-slate-400">Chưa có tài khoản? </Text>
                            <Link to="/register" className="text-primary hover:text-blue-400 font-medium">Đăng ký ngay</Link>
                        </div>
                        <Text className="text-slate-500 block">Hệ thống quản lý robot tự động hóa</Text>
                    </div>
                </Form>
            </Card>
        </div>
    );
};

export default LoginPage;
