import React, { useState } from 'react';
import { Form, Input, Button, Card, message, Typography } from 'antd';
import { UserOutlined, LockOutlined, MailOutlined, RocketOutlined, ArrowLeftOutlined } from '@ant-design/icons';
import api from '../api/axios';
import { useNavigate, Link } from 'react-router-dom';

const { Title, Text } = Typography;

const RegisterPage: React.FC = () => {
    const [loading, setLoading] = useState(false);
    const navigate = useNavigate();

    const onFinish = async (values: any) => {
        setLoading(true);
        try {
            await api.post('/auth/register', values);
            message.success('Đăng ký thành công! Vui lòng đăng nhập.');
            navigate('/login');
        } catch (error: any) {
            message.error(error.response?.data || 'Đăng ký thất bại.');
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
                <div className="text-center mb-6">
                    <div className="inline-flex items-center justify-center w-12 h-12 rounded-xl bg-primary text-white text-2xl mb-4 shadow-lg shadow-blue-500/50">
                        <RocketOutlined />
                    </div>
                    <Title level={3} className="!text-white !m-0 !mb-1">Tạo tài khoản</Title>
                    <Text className="text-slate-400">Đăng ký để quản trị hệ thống RPA</Text>
                </div>

                <Form
                    name="register"
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
                        name="email"
                        rules={[
                            { required: true, message: 'Vui lòng nhập email!' },
                            { type: 'email', message: 'Email không hợp lệ!' }
                        ]}
                    >
                        <Input
                            prefix={<MailOutlined className="text-slate-400" />}
                            placeholder="Email"
                            className="!bg-slate-700 !border-slate-600 !text-white hover:!border-primary focus:!border-primary"
                        />
                    </Form.Item>

                    <Form.Item
                        name="password"
                        rules={[
                            { required: true, message: 'Vui lòng nhập mật khẩu!' },
                            { min: 6, message: 'Mật khẩu phải từ 6 ký tự trở lên!' }
                        ]}
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
                            Đăng ký ngay
                        </Button>
                    </Form.Item>

                    <div className="text-center">
                        <Link to="/login" className="text-primary hover:text-blue-400 flex items-center justify-center gap-2">
                            <ArrowLeftOutlined /> Quay lại đăng nhập
                        </Link>
                    </div>
                </Form>
            </Card>
        </div>
    );
};

export default RegisterPage;
