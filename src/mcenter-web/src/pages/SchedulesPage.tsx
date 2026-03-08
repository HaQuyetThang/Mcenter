import React, { useEffect, useState } from 'react';
import { Table, Card, Button, Typography, Space, message, Tag, Switch, Modal, Form, Input, Select } from 'antd';
import { ReloadOutlined, PlusOutlined, CalendarOutlined } from '@ant-design/icons';
import api from '../api/axios';
import type { Schedule, Workflow, Server } from '../types';
import dayjs from 'dayjs';

const { Title } = Typography;
const { Option } = Select;

const SchedulesPage: React.FC = () => {
    const [schedules, setSchedules] = useState<Schedule[]>([]);
    const [workflows, setWorkflows] = useState<Workflow[]>([]);
    const [servers, setServers] = useState<Server[]>([]);
    const [loading, setLoading] = useState(false);
    const [isModalVisible, setIsModalVisible] = useState(false);
    const [editingSchedule, setEditingSchedule] = useState<Schedule | null>(null);
    const [confirmLoading, setConfirmLoading] = useState(false);
    const [form] = Form.useForm();

    const fetchData = async () => {
        setLoading(true);
        try {
            const [schedulesRes, workflowsRes, serversRes] = await Promise.all([
                api.get('/schedules'),
                api.get('/workflows'),
                api.get('/servers')
            ]);
            setSchedules(schedulesRes.data);
            setWorkflows(workflowsRes.data);
            setServers(serversRes.data);
        } catch (error) {
            message.error('Không thể tải dữ liệu.');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchData();
    }, []);

    const handleSaveSchedule = async (values: any) => {
        setConfirmLoading(true);
        try {
            if (editingSchedule) {
                await api.put(`/schedules/${editingSchedule.id}`, { ...values, isActive: editingSchedule.isActive });
                message.success('Cập nhật lịch trình thành công!');
            } else {
                await api.post('/schedules', { ...values, isActive: true });
                message.success('Tạo lịch trình thành công!');
            }
            setIsModalVisible(false);
            setEditingSchedule(null);
            form.resetFields();
            fetchData();
        } catch (error: any) {
            message.error(error.response?.data?.message || 'Thao tác thất bại.');
        } finally {
            setConfirmLoading(false);
        }
    };

    const handleOpenAdd = () => {
        setEditingSchedule(null);
        form.resetFields();
        setIsModalVisible(true);
    };

    const handleOpenEdit = (schedule: Schedule) => {
        setEditingSchedule(schedule);
        form.setFieldsValue(schedule);
        setIsModalVisible(true);
    };

    const handleDeleteSchedule = async (id: number) => {
        Modal.confirm({
            title: 'Xác nhận xóa',
            content: 'Bạn có chắc chắn muốn xóa lịch trình này không?',
            onOk: async () => {
                try {
                    await api.delete(`/schedules/${id}`);
                    message.success('Xóa lịch trình thành công!');
                    fetchData();
                } catch (error) {
                    message.error('Không thể xóa lịch trình.');
                }
            }
        });
    };

    const handleToggleActive = async (id: number) => {
        try {
            await api.post(`/schedules/${id}/toggle`);
            message.success('Cập nhật trạng thái thành công!');
            fetchData();
        } catch (error) {
            message.error('Không thể cập nhật trạng thái lịch trình.');
        }
    };

    const columns = [
        {
            title: 'Tên lịch trình',
            dataIndex: 'name',
            key: 'name',
            render: (text: string) => (
                <Space>
                    <CalendarOutlined className="text-primary" />
                    <span className="font-semibold">{text}</span>
                </Space>
            ),
        },
        {
            title: 'Workflow',
            dataIndex: 'workflowName',
            key: 'workflowName',
        },
        {
            title: 'Cron Expression',
            dataIndex: 'cronExpression',
            key: 'cronExpression',
            render: (cron: string) => <code className="bg-slate-100 p-1 rounded text-primary font-mono">{cron}</code>,
        },
        {
            title: 'Máy chủ chỉ định',
            dataIndex: 'serverIds',
            key: 'serverIds',
            render: (ids: number[]) => (
                <Space size={[0, 4]} wrap>
                    {ids?.map(id => (
                        <Tag key={id} color="cyan">Server #{id}</Tag>
                    ))}
                </Space>
            ),
        },
        {
            title: 'Lần chạy tới',
            dataIndex: 'nextRunTime',
            key: 'nextRunTime',
            render: (time: string) => time ? dayjs(time).format('DD/MM/YYYY HH:mm:ss') : '-',
        },
        {
            title: 'Kích hoạt',
            dataIndex: 'isActive',
            key: 'isActive',
            render: (active: boolean, record: Schedule) => (
                <Switch checked={active} onChange={() => handleToggleActive(record.id)} />
            ),
        },
        {
            title: 'Thao tác',
            key: 'action',
            render: (_: any, record: Schedule) => (
                <Space size="middle">
                    <Button type="link" onClick={() => handleOpenEdit(record)}>Sửa</Button>
                    <Button type="link" danger onClick={() => handleDeleteSchedule(record.id)}>Xóa</Button>
                </Space>
            ),
        },
    ];

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <Title level={2} className="!m-0">Lịch trình thực thi</Title>
                    <Typography.Text className="text-slate-500">Thiết lập thời gian và máy chủ chạy tự động cho các workflow</Typography.Text>
                </div>
                <Space>
                    <Button icon={<ReloadOutlined />} onClick={fetchData} loading={loading}>Làm mới</Button>
                    <Button
                        type="primary"
                        icon={<PlusOutlined />}
                        className="shadow-lg shadow-blue-500/30"
                        onClick={handleOpenAdd}
                    >
                        Tạo lịch mới
                    </Button>
                </Space>
            </div>

            <Card className="shadow-sm border-0">
                <Table columns={columns} dataSource={schedules} rowKey="id" loading={loading} />
            </Card>

            <Modal
                title={editingSchedule ? "Chỉnh sửa lịch trình" : "Tạo lịch trình mới"}
                open={isModalVisible}
                onOk={() => form.submit()}
                onCancel={() => setIsModalVisible(false)}
                confirmLoading={confirmLoading}
                destroyOnClose
                width={600}
            >
                <Form
                    form={form}
                    layout="vertical"
                    onFinish={handleSaveSchedule}
                >
                    <Form.Item
                        name="name"
                        label="Tên lịch trình"
                        rules={[{ required: true, message: 'Vui lòng nhập tên lịch trình!' }]}
                    >
                        <Input placeholder="Ví dụ: Daily Invoice Job" />
                    </Form.Item>

                    <Form.Item
                        name="workflowId"
                        label="Chọn Workflow"
                        rules={[{ required: true, message: 'Vui lòng chọn workflow!' }]}
                    >
                        <Select placeholder="Chọn quy trình RPA">
                            {workflows.map(wf => (
                                <Option key={wf.id} value={wf.id}>{wf.name} (v{wf.version})</Option>
                            ))}
                        </Select>
                    </Form.Item>

                    <Card size="small" className="bg-slate-50 border-dashed mb-4">
                        <Typography.Text strong>Trình tạo lịch chạy (Cron Builder)</Typography.Text>
                        <div className="mt-2 grid grid-cols-2 gap-4">
                            <Form.Item label="Chọn nhanh">
                                <Select
                                    defaultValue="daily"
                                    onChange={(val) => {
                                        const mapping: any = {
                                            'm5': '*/5 * * * *',
                                            'm15': '*/15 * * * *',
                                            'h1': '0 * * * *',
                                            'd0': '0 0 * * *',
                                            'w0': '0 0 * * 0'
                                        };
                                        form.setFieldsValue({ cronExpression: mapping[val] });
                                    }}
                                >
                                    <Option value="m5">Mỗi 5 phút</Option>
                                    <Option value="m15">Mỗi 15 phút</Option>
                                    <Option value="h1">Mỗi giờ (vào phút số 0)</Option>
                                    <Option value="d0">Hàng ngày (vào lúc 00:00)</Option>
                                    <Option value="w0">Hàng tuần (Chủ nhật 00:00)</Option>
                                </Select>
                            </Form.Item>

                            <Form.Item
                                name="cronExpression"
                                label="Mã Cron (Kết quả)"
                                rules={[{ required: true, message: 'Vui lòng nhập mã Cron!' }]}
                            >
                                <Input placeholder="0 0 * * *" />
                            </Form.Item>
                        </div>
                        <Typography.Text type="secondary" style={{ fontSize: '11px' }}>
                            Mẹo: Hệ thống sử dụng chuẩn Cron 5 trường. Bạn có thể tự sửa mã Cron ở trên nếu cần tùy chỉnh nâng cao.
                        </Typography.Text>
                    </Card>

                    <Form.Item
                        name="serverIds"
                        label="Gán máy chủ thực thi"
                        rules={[{ required: true, message: 'Vui lòng chọn ít nhất một máy chủ!' }]}
                    >
                        <Select mode="multiple" placeholder="Chọn các máy chủ">
                            {servers.map(srv => (
                                <Option key={srv.id} value={srv.id}>{srv.name} ({srv.ipAddress})</Option>
                            ))}
                        </Select>
                    </Form.Item>
                </Form>
            </Modal>
        </div>
    );
};

export default SchedulesPage;
