import sys
import types
import asyncio

sys.path.append('vllm-service')

# Provide dummy modules for heavy dependencies if not installed
if 'torch' not in sys.modules:
    sys.modules['torch'] = types.ModuleType('torch')
if 'vllm' not in sys.modules:
    fake_vllm = types.ModuleType('vllm')
    fake_vllm.LLM = object
    fake_vllm.SamplingParams = object
    sys.modules['vllm'] = fake_vllm
if 'transformers' not in sys.modules:
    fake_transformers = types.ModuleType('transformers')
    fake_transformers.AutoTokenizer = object
    sys.modules['transformers'] = fake_transformers

from main import health_check


def test_health_endpoint():
    result = asyncio.run(health_check())
    assert result['status'] == 'healthy'
