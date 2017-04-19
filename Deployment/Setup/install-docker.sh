pushd Playbooks
export TERRAFORM_STATE_ROOT=../../Provision
ansible-galaxy install angstwad.docker_ubuntu
ansible-playbook -i terraform.py -s install-docker.yml
popd
