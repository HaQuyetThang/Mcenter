import React, { useEffect, useState } from 'react';
import { Table, Card, Button, Typography, Space, message, Tag, Modal } from 'antd';
import { ReloadOutlined, EyeOutlined } from '@ant-design/icons';
import api from '../api/axios';
import type { Execution } from '../types';
import dayjs from 'dayjs';

const { Title } = Typography;

/** Trả về màu cho từng dòng log theo level */
const getLineColor = (line: string): string => {
    const lower = line.toLowerCase();
    if (lower.includes('[error]') || lower.includes('[critical]')) return '#ff6b6b';
    if (lower.includes('[warning]')) return '#ffd93d';
    if (lower.includes('[information]')) return '#a8ff78';
    return '#e0e0e0';
};

const ExecutionsPage: React.FC = () => {
    const [executions, setExecutions] = useState<Execution[]>([]);
    const [loading, setLoading] = useState(false);
    const [logModalVisible, setLogModalVisible] = useState(false);
    const [selectedLog, setSelectedLog] = useState('');
    const [selectedTitle, setSelectedTitle] = useState('');

    const fetchExecutions = async () => {
        setLoading(true);
        try {
            const response = await api.get('/executions');
            setExecutions(response.data);
        } catch (error) {
            message.error('Không thể tải lịch sử thực thi.');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchExecutions();
        const interval = setInterval(fetchExecutions, 10000);
        return () => clearInterval(interval);
    }, []);

    const showLogs = (record: Execution) => {
        setSelectedLog(record.logOutput || 'Không có log nào được ghi lại.');
        setSelectedTitle(`Log: ${record.workflowName} — ${dayjs(record.startTime).format('HH:mm:ss DD/MM/YYYY')}`);
        setLogModalVisible(true);
    };

    const columns = [
        {
            title: 'Workflow',
            dataIndex: 'workflowName',
            key: 'workflowName',
            render: (text: string) => <span className="font-semibold">{text}</span>,
        },
        {
            title: 'Máy chủ',
            dataIndex: 'serverName',
            key: 'serverName',
            render: (text: string) => text || 'Global',
        },
        {
            title: 'Trạng thái',
            dataIndex: 'status',
            key: 'status',
            render: (status: string) => {
                const colorMap: Record<string, string> = {
                    'Success': 'success',
                    'Failed': 'error',
                    'Running': 'processing',
                    'Queued': 'warning',
                    'Cancelled': 'default',
                };
                return <Tag color={colorMap[status] ?? 'default'}>{status}</Tag>;
            },
        },
        {
            title: 'Bắt đầu',
            dataIndex: 'startTime',
            key: 'startTime',
            render: (time: string) => time ? dayjs(time).format('HH:mm:ss DD/MM') : '-',
        },
        {
            title: 'Kết thúc',
            dataIndex: 'endTime',
            key: 'endTime',
            render: (time: string) => time ? dayjs(time).format('HH:mm:ss DD/MM') : '-',
        },
        {
            title: 'Kích hoạt bởi',
            dataIndex: 'triggeredBy',
            key: 'triggeredBy',
        },
        {
            title: 'Logs',
            key: 'logs',
            render: (_: any, record: Execution) => (
                <Button
                    type="link"
                    icon={<EyeOutlined />}
                    onClick={() => showLogs(record)}
                    disabled={record.status === 'Queued' || record.status === 'Cancelled'}
                >
                    Xem Log
                </Button>
            ),
        },
    ];

    const logLines = selectedLog.split('\n').filter(Boolean);

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <Title level={2} className="!m-0">Lịch sử thực thi</Title>
                    <Typography.Text className="text-slate-500">Giám sát quá trình chạy và xem log chi tiết của từng máy chủ</Typography.Text>
                </div>
                <Space>
                    <Button icon={<ReloadOutlined />} onClick={fetchExecutions} loading={loading}>Làm mới</Button>
                </Space>
            </div>

            <Card className="shadow-sm border-0">
                <Table columns={columns} dataSource={executions} rowKey="id" loading={loading} />
            </Card>

            <Modal
                title={selectedTitle}
                open={logModalVisible}
                onCancel={() => setLogModalVisible(false)}
                footer={[
                    <Button key="close" onClick={() => setLogModalVisible(false)}>Đóng</Button>
                ]}
                width={860}
            >
                <div
                    style={{
                        backgroundColor: '#1a1a2e',
                        borderRadius: 8,
                        padding: '12px 16px',
                        maxHeight: 480,
                        overflowY: 'auto',
                        fontFamily: 'Consolas, "Courier New", monospace',
                        fontSize: 12,
                        lineHeight: '1.7',
                    }}
                >
                    {logLines.length === 0 ? (
                        <span style={{ color: '#888' }}>Không có log nào.</span>
                    ) : (
                        logLines.map((line, idx) => (
                            <div key={idx} style={{ color: getLineColor(line) }}>{line}</div>
                        ))
                    )}
                </div>
            </Modal>
        </div>
    );
};

export default ExecutionsPage;
